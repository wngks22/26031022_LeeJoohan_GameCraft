abstract class Attack : MonoBehaviour
{
	protected readonly CombatWorld World;
	public SpriteRenderer Renderer { get; set; } = null!;
	public Animator Animator { get; set; } = null!;
	protected float Age;
	protected float Damage;
	protected Attack(CombatWorld world) { World = world; }
	protected void Reset(float damage)
	{
		Age = 0;
		Damage = damage;
		Renderer.Opacity = 1;
		Renderer.FlipX = false;
		Transform.Rotation = 0;
		GameObject.SetActive(true);
	}
	public abstract void CheckHits();
}

class BoltAttack : Attack
{
	public CircleCollider Collider { get; set; } = null!;
	private readonly List<int> _hitIds = new List<int>();
	private readonly List<Enemy> _nearby = new List<Enemy>();
	private float _dx, _dy, _speed, _range, _lifetime, _previousX, _previousY;
	private int _maxHits;
	private bool _expired;
	public BoltAttack(CombatWorld world) : base(world) { }
	public void Launch(float angle, WeaponStats stats, float damage)
	{
		Reset(damage);
		_hitIds.Clear();
		_expired = false;
		_dx = MathF.Cos(angle);
		_dy = MathF.Sin(angle);
		_speed = stats.Speed;
		_range = stats.MaxDistance;
		_lifetime = stats.Lifetime;
		_maxHits = stats.MaxHits;
		// 56x48 시트의 밝은 탄두 중심은 약 (39,26), 피벗은 (28,24)입니다.
		Collider.Radius = 10;
		Collider.OffsetX = 11;
		Collider.OffsetY = 2;
		Transform.X = World.PlayerCollider.CenterX;
		Transform.Y = World.PlayerCollider.CenterY;
		Transform.Rotation = angle * 180 / MathF.PI;
		_previousX = Collider.CenterX;
		_previousY = Collider.CenterY;
		Animator.Play(World.Assets.Clip("magic_bolt"));
	}
	public override void Update(float deltaTime)
	{
		_previousX = Collider.CenterX;
		_previousY = Collider.CenterY;
		float remaining = MathF.Min(_lifetime - Age, _range / _speed - Age);
		float movementTime = MathF.Max(0, MathF.Min(deltaTime, remaining));
		Transform.X += _dx * _speed * movementTime;
		Transform.Y += _dy * _speed * movementTime;
		Age += deltaTime;
		_expired = Age * _speed >= _range || Age >= _lifetime;
	}
	public override void CheckHits()
	{
		float travel = MathF.Sqrt(CombatMath.DistanceSquared(_previousX, _previousY, Collider.CenterX, Collider.CenterY));
		World.Grid.Query(Collider.CenterX, Collider.CenterY, travel + 65, _nearby);
		// 진행 방향의 가까운 적부터 맞혀 관통 횟수가 순회 순서에 의존하지 않게 합니다.
		_nearby.Sort(CompareDistance);
		foreach (Enemy enemy in _nearby)
		{
			if (!enemy.IsAlive || _hitIds.Contains(enemy.SpawnId)) continue;
			if (!CombatMath.SegmentHits(_previousX, _previousY, Collider.CenterX, Collider.CenterY, enemy.Collider.CenterX, enemy.Collider.CenterY, enemy.Collider.WorldRadius + Collider.WorldRadius)) continue;
			_hitIds.Add(enemy.SpawnId);
			World.ApplyDamage(enemy, Damage);
			if (_hitIds.Count >= _maxHits) { GameObject.SetActive(false); break; }
		}
		if (_expired) GameObject.SetActive(false);
	}
	private int CompareDistance(Enemy a, Enemy b)
	{
		float da = CombatMath.DistanceSquared(_previousX, _previousY, a.Collider.CenterX, a.Collider.CenterY);
		float db = CombatMath.DistanceSquared(_previousX, _previousY, b.Collider.CenterX, b.Collider.CenterY);
		return da.CompareTo(db);
	}
}

class SwingAttack : Attack
{
	private readonly List<int> _hitIds = new List<int>();
	private readonly List<Enemy> _nearby = new List<Enemy>();
	private WeaponStats _stats = null!;
	private AnimationClip _clip = null!;
	private bool _left;
	private float _previousAngle;
	private float _previousPivotX, _previousPivotY;
	private bool _large;
	public SwingAttack(CombatWorld world) : base(world) { }
	public void Swing(bool left, int level, WeaponStats stats, float damage)
	{
		Reset(damage);
		_hitIds.Clear();
		_left = left;
		_stats = stats;
		_large = level >= 4;
		_clip = World.Assets.Clip(_large ? "scythe_large" : "scythe_small");
		Animator.IsPlaying = false;
		Renderer.FlipX = left;
		_previousAngle = -stats.Sweep / 2;
		Update(0);
		_previousPivotX = Transform.X;
		_previousPivotY = Transform.Y;
	}
	public override void Update(float deltaTime)
	{
		_previousPivotX = Transform.X;
		_previousPivotY = Transform.Y;
		_previousAngle = -_stats.Sweep / 2 + MathF.Min(_stats.Sweep, Age * _stats.Speed);
		Age += deltaTime;
		float duration = _stats.Sweep / _stats.Speed;
		float progress = Math.Clamp(Age / duration, 0, 1);
		int frame = Math.Min(_clip.FrameCount - 1, (int)(progress * (_clip.FrameCount - 1)));
		float u = frame / (float)(_clip.FrameCount - 1);
		// 시트에 이미 그려진 회전량을 빼야 원하는 각도로 한 번만 회전합니다.
		float baked = -62 + 110 * u * u * (3 - 2 * u);
		float angle = -_stats.Sweep / 2 + MathF.Min(_stats.Sweep, Age * _stats.Speed);
		Transform.X = World.Player.Transform.X + (_left ? -10 : 10);
		Transform.Y = World.Player.Transform.Y - 30;
		Transform.Rotation = (angle - baked) * (_left ? -1 : 1);
		Renderer.Sprite = _clip.GetFrame(frame);
		if (Age > duration) Renderer.Opacity = MathF.Max(0, 1 - (Age - duration) / 0.09f);
		if (Age >= duration + 0.09f) GameObject.SetActive(false);
	}
	public override void CheckHits()
	{
		if (_previousAngle >= _stats.Sweep / 2 && Age > _stats.Sweep / _stats.Speed) return;
		float travel = MathF.Sqrt(CombatMath.DistanceSquared(_previousPivotX, _previousPivotY, Transform.X, Transform.Y));
		float reach = WeaponGeometry.ScytheFront(_large) + WeaponGeometry.ScytheRadius(_large) + travel + 64;
		World.Grid.Query(Transform.X, Transform.Y, reach, _nearby);
		foreach (Enemy enemy in _nearby)
		{
			if (!enemy.IsAlive || _hitIds.Contains(enemy.SpawnId)) continue;
			if (!CapsuleHits(enemy)) continue;
			_hitIds.Add(enemy.SpawnId);
			World.ApplyDamage(enemy, Damage);
		}
	}

	// 손잡이 뒤부터 날까지 캡슐 하나로 검사하며, 빠른 회전 사이도 검사합니다.
	private bool CapsuleHits(Enemy enemy)
	{
		float end = -_stats.Sweep / 2 + MathF.Min(_stats.Sweep, Age * _stats.Speed);
		int steps = Math.Max(1, (int)MathF.Ceiling(MathF.Abs(end - _previousAngle) / 5));
		float back = WeaponGeometry.ScytheBack(_large);
		float front = WeaponGeometry.ScytheFront(_large);
		float frontY = WeaponGeometry.ScytheFrontY(_large);
		float radius = WeaponGeometry.ScytheRadius(_large) + enemy.Collider.WorldRadius;
		for (int i = 0; i <= steps; i++)
		{
			float ratio = i / (float)steps;
			float angle = (_previousAngle + (end - _previousAngle) * ratio) * MathF.PI / 180;
			float dx = MathF.Cos(angle) * (_left ? -1 : 1), dy = MathF.Sin(angle);
			float x = _previousPivotX + (Transform.X - _previousPivotX) * ratio;
			float y = _previousPivotY + (Transform.Y - _previousPivotY) * ratio;
			float startX = x - dx * back;
			float startY = y - dy * back;
			float endX = x + dx * front - MathF.Sin(angle) * frontY * (_left ? -1 : 1);
			float endY = y + dy * front + MathF.Cos(angle) * frontY;
			if (CombatMath.SegmentHits(startX, startY, endX, endY,
				enemy.Collider.CenterX, enemy.Collider.CenterY, radius)) return true;
		}
		return false;
	}
}
