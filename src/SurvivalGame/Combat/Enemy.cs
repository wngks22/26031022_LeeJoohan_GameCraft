// 종류마다 이동만 재정의하고 체력·피격·사망 처리는 공유합니다.
abstract class Enemy : MonoBehaviour, IDamageable
{
	protected readonly CombatWorld World;
	public EnemyKind Kind { get; }
	public SpriteRenderer Renderer { get; set; } = null!;
	public Animator Animator { get; set; } = null!;
	public HitFlash Flash { get; set; } = null!;
	public CircleCollider Collider { get; set; } = null!;
	public AnimationClip MoveClip { get; set; } = null!;
	public AnimationClip DeathClip { get; set; } = null!;
	public float Health { get; private set; }
	public float ContactDamage { get; private set; }
	public float Speed { get; private set; }
	public float SlowMultiplier { get; set; } = 1;
	public float NextFireHitTime { get; set; }
	public int Experience { get; private set; }
	public int SpawnId { get; private set; }
	public bool DeathProcessed { get; private set; }
	public bool IsAlive => GameObject.ActiveSelf && Health > 0;
	protected Enemy(CombatWorld world, EnemyKind kind) { World = world; Kind = kind; }
	public void Spawn(float x, float y, float difficulty, int spawnId)
	{
		Transform.X = x;
		Transform.Y = y;
		Transform.Rotation = 0;
		SpawnId = spawnId;
		DeathProcessed = false;
		SlowMultiplier = 1;
		Health = 50;
		Speed = 58;
		ContactDamage = 10;
		Experience = 3;
		if (Kind == EnemyKind.Bat) { Health = 20; Speed = 220; ContactDamage = 8; Experience = 2; }
		if (Kind == EnemyKind.Flower) { Health = 170; Speed = 12; ContactDamage = 16; Experience = 8; }
		if (Kind == EnemyKind.Boss) { Health = 1100; Speed = 45; ContactDamage = 24; Experience = 0; }
		Health *= difficulty;
		ContactDamage *= GameBalance.ContactDifficulty(World.Time);
		Flash.Clear();
		Renderer.Opacity = 1;
		Renderer.FlipX = false;
		GameObject.SetActive(true);
		Animator.Play(MoveClip);
		Animator.Update(World.Random.NextSingle() * MoveClip.FrameSeconds * MoveClip.FrameCount);
	}
	public override void Update(float deltaTime)
	{
		if (!IsAlive)
		{
			if (DeathProcessed && Animator.IsFinished) GameObject.SetActive(false);
			return;
		}
		Move(deltaTime);
	}
	protected abstract void Move(float deltaTime);
	protected void FollowPlayer(float deltaTime)
	{
		float dx = World.Player.Transform.X - Transform.X;
		float dy = World.Player.Transform.Y - Transform.Y;
		float length = MathF.Sqrt(dx * dx + dy * dy);
		if (length < 1) return;
		Transform.X += dx / length * Speed * SlowMultiplier * deltaTime;
		Transform.Y += dy / length * Speed * SlowMultiplier * deltaTime;
		// 원본 적 시트는 왼쪽을 향합니다.
		if (dx != 0) Renderer.FlipX = dx > 0;
	}
	public void TakeDamage(float damage)
	{
		if (!IsAlive || damage <= 0) return;
		Health = MathF.Max(0, Health - damage);
		Flash.Play(GameBalance.EnemyFlashSeconds);
		World.Audio.Play("enemy_hit");
	}
	public void BeginDeath()
	{
		if (DeathProcessed) return;
		DeathProcessed = true;
		Flash.Clear();
		Renderer.FlashTexture = null;
		Animator.Play(DeathClip);
	}
}

class FollowingEnemy : Enemy
{
	public FollowingEnemy(CombatWorld world, EnemyKind kind) : base(world, kind) { }
	protected override void Move(float deltaTime) { FollowPlayer(deltaTime); }
}

class CrossingEnemy : Enemy
{
	public float DirectionX { get; set; }
	public float DirectionY { get; set; }
	public CrossingEnemy(CombatWorld world) : base(world, EnemyKind.Bat) { }
	protected override void Move(float deltaTime)
	{
		Transform.X += DirectionX * Speed * SlowMultiplier * deltaTime;
		Transform.Y += DirectionY * Speed * SlowMultiplier * deltaTime;
		Renderer.FlipX = DirectionX > 0;
	}
}
