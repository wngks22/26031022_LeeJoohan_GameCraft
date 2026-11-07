class CombatWorld
{
	private readonly GameplayScene _scene;
	private readonly List<Attack> _attacks = new List<Attack>();
	private readonly List<ExperienceGem> _gems = new List<ExperienceGem>();
	private readonly List<Enemy> _nearby = new List<Enemy>();
	private readonly List<Enemy> _targetCandidates = new List<Enemy>();
	private readonly List<PendingTargetHit> _targetHits = new List<PendingTargetHit>();
	private int _spawnId;
	private int _experiencePending;
	private int _bossPending;
	private FireAreaAttack? _fireAttack;
	private readonly List<LightningEffect> _lightningEffects = new List<LightningEffect>();
	public List<Enemy> Enemies { get; } = new List<Enemy>();
	public EnemyGrid Grid { get; } = new EnemyGrid();
	public Random Random { get; } = new Random();
	public GameAssets Assets { get; }
	public GameAudio Audio { get; }
	public GameObject Player { get; }
	public PlayerController Controller { get; }
	public PlayerHealth Health { get; }
	public CircleCollider PlayerCollider { get; }
	public WeaponInventory Inventory { get; }
	public RewardSystem Rewards { get; }
	public float Time { get; private set; }
	public CombatWorld(GameplayScene scene, GameAssets assets, GameAudio audio, GameObject player)
	{
		_scene = scene;
		Assets = assets;
		Audio = audio;
		Player = player;
		Controller = player.GetComponent<PlayerController>()!;
		Health = player.GetComponent<PlayerHealth>()!;
		PlayerCollider = player.GetComponent<CircleCollider>()!;
		Inventory = player.AddComponent(new WeaponInventory(this));
		Rewards = new RewardSystem(this);
		Inventory.AddOrUpgrade(WeaponKind.MagicBolt);
	}
	public void Update(float deltaTime)
	{
		Time += deltaTime;
		GameManager.Instance.AddPlayTime(deltaTime);
		Grid.Rebuild(Enemies);
		Inventory.UpdateWeapons(deltaTime);
	}
	public int ActiveEnemyCount()
	{
		int count = 0;
		foreach (Enemy enemy in Enemies) if (enemy.GameObject.ActiveSelf) count++;
		return count;
	}
	public Enemy? SpawnEnemy(EnemyKind kind, float x, float y)
	{
		if (ActiveEnemyCount() >= GameBalance.EnemyLimit) return null;
		Enemy? enemy = null;
		foreach (Enemy candidate in Enemies)
			if (candidate.Kind == kind && !candidate.GameObject.ActiveSelf) { enemy = candidate; break; }
		if (enemy == null)
		{
			GameObject obj = new GameObject(kind.ToString());
			if (kind == EnemyKind.Bat) enemy = obj.AddComponent(new CrossingEnemy(this));
			else enemy = obj.AddComponent(new FollowingEnemy(this, kind));
			enemy.Renderer = obj.AddComponent(new SpriteRenderer(Assets.Frame("skeleton_move")));
			enemy.Renderer.Order = kind == EnemyKind.Bat ? 10 : 0;
			enemy.Animator = obj.AddComponent(new Animator(enemy.Renderer, Assets.Clip("skeleton_move")));
			enemy.Flash = obj.AddComponent(new HitFlash(enemy.Renderer));
			enemy.Collider = obj.AddComponent(new CircleCollider(kind == EnemyKind.Boss ? 27 : 18, kind == EnemyKind.Bat ? -24 : -20));
			Enemies.Add(enemy);
			_scene.AddGameObject(obj);
		}
		string name = "skeleton";
		if (kind == EnemyKind.Bat) name = "bat";
		if (kind == EnemyKind.Boss) name = "boss";
		if (kind == EnemyKind.Flower) name = Random.Next(2) == 0 ? "flower_red" : "flower_pink";
		enemy.MoveClip = Assets.Clip(name + "_move"); enemy.DeathClip = Assets.Clip(name + "_death");
		enemy.Renderer.FlashTexture = Assets.Frame(name + "_move_hitflash").Texture;
		enemy.NextFireHitTime = 0;
		enemy.Spawn(x, y, GameBalance.Difficulty(Time), ++_spawnId);
		return enemy;
	}
	public Enemy? FindNearest(float range)
	{
		return FindNearest(range, null);
	}
	public Enemy? FindNearest(float range, List<int>? excluded, bool insideScreen = false)
	{
		Enemy? result = null;
		float distance = range * range;
		foreach (Enemy enemy in Enemies)
		{
			if (!enemy.IsAlive) continue;
			if (excluded != null && excluded.Contains(enemy.SpawnId)) continue;
			if (insideScreen && !IsInsideTargetArea(enemy)) continue;
			float d = CombatMath.DistanceSquared(PlayerCollider.CenterX, PlayerCollider.CenterY, enemy.Collider.CenterX, enemy.Collider.CenterY);
			if (d <= distance) { result = enemy; distance = d; }
		}
		return result;
	}
	public Enemy? FindRandom(float range, List<int> excluded, bool insideScreen = false)
	{
		_targetCandidates.Clear();
		foreach (Enemy enemy in Enemies)
		{
			if (!enemy.IsAlive || excluded.Contains(enemy.SpawnId)) continue;
			if (insideScreen && !IsInsideTargetArea(enemy)) continue;
			if (CombatMath.DistanceSquared(PlayerCollider.CenterX, PlayerCollider.CenterY, enemy.Collider.CenterX, enemy.Collider.CenterY) > range * range) continue;
			_targetCandidates.Add(enemy);
		}
		if (_targetCandidates.Count == 0) return null;
		return _targetCandidates[Random.Next(_targetCandidates.Count)];
	}
	private bool IsInsideTargetArea(Enemy enemy)
	{
		Camera? camera = _scene.MainCamera;
		if (camera == null) return false;
		// 낙뢰 착탄점(발 위치)이 실제 카메라 화면 안쪽 80px에 들어왔는지 확인합니다.
		float x = camera.WorldToScreenX(enemy.Collider.CenterX);
		float y = camera.WorldToScreenY(enemy.Transform.Y);
		return x >= 80 && x <= GameGlobal.ScreenSize.Width - 80
			&& y >= 80 && y <= GameGlobal.ScreenSize.Height - 80;
	}
	private T? FindAttack<T>() where T : Attack
	{
		foreach (Attack attack in _attacks)
			if (attack is T matched && !matched.GameObject.ActiveSelf) return matched;
		return null;
	}
	private void RegisterAttack(Attack attack, string clipName)
	{
		GameObject obj = new GameObject(clipName);
		obj.AddComponent(attack);
		attack.Renderer = obj.AddComponent(new SpriteRenderer(Assets.Frame(clipName)));
		attack.Renderer.SortingLayer = 4;
		attack.Animator = obj.AddComponent(new Animator(attack.Renderer, Assets.Clip(clipName)));
		if (attack is BoltAttack bolt) bolt.Collider = obj.AddComponent(new CircleCollider(10));
		_attacks.Add(attack);
		_scene.AddGameObject(obj);
	}
	private bool CanAddAttack()
	{
		int count = 0;
		foreach (Attack attack in _attacks) if (attack.GameObject.ActiveSelf) count++;
		foreach (LightningEffect effect in _lightningEffects)
			if (effect.GameObject.ActiveSelf) count++;
		return count < GameBalance.AttackLimit;
	}
	public void SpawnBolt(float angle, WeaponStats stats, float damage)
	{
		if (!CanAddAttack()) return;
		BoltAttack? attack = FindAttack<BoltAttack>();
		if (attack == null) { attack = new BoltAttack(this); RegisterAttack(attack, "magic_bolt"); }
		attack.Launch(angle, stats, damage); Audio.Play("magic_bolt");
	}
	public void SpawnSwing(bool left, int level, WeaponStats stats, float damage)
	{
		if (!CanAddAttack()) return;
		SwingAttack? attack = FindAttack<SwingAttack>();
		if (attack == null) { attack = new SwingAttack(this); RegisterAttack(attack, "scythe_small"); }
		attack.Swing(left, level, stats, damage); Audio.Play("scythe");
	}
	public void SpawnLightning(Enemy target, float damage)
	{
		// 시각 효과의 상한에 걸려도 이미 선택한 공격의 피해는 유지합니다.
		_targetHits.Add(new PendingTargetHit(target, damage)); Audio.Play("lightning");
		if (!CanAddAttack()) return;
		LightningEffect? effect = null;
		foreach (LightningEffect candidate in _lightningEffects)
		{
			if (!candidate.GameObject.ActiveSelf)
			{
				effect = candidate;
				break;
			}
		}
		if (effect == null)
		{
			GameObject obj = new GameObject("LightningEffect");
			effect = obj.AddComponent(new LightningEffect(Assets.Clip("lightning")));
			effect.Renderer = obj.AddComponent(new SpriteRenderer(Assets.Frame("lightning")));
			effect.Renderer.SortingLayer = 4;
			effect.Animator = obj.AddComponent(new Animator(effect.Renderer, Assets.Clip("lightning")));
			_lightningEffects.Add(effect);
			_scene.AddGameObject(obj);
		}
		effect.Show(target.Collider.CenterX, target.Transform.Y);
	}
	public void UpdateFire(AreaWeapon weapon)
	{
		if (_fireAttack == null)
		{
			GameObject obj = new GameObject("FireArea");
			_fireAttack = obj.AddComponent(new FireAreaAttack(this));
			_fireAttack.Renderer = obj.AddComponent(new SpriteRenderer(Assets.Frame("fire_area")));
			_fireAttack.Renderer.SortingLayer = 1;
			_fireAttack.Animator = obj.AddComponent(new Animator(_fireAttack.Renderer, Assets.Clip("fire_area")));
			_scene.AddGameObject(obj);
		}
		_fireAttack.Refresh(weapon);
	}
	public void CheckCollisions(float deltaTime)
	{
		SeparateEnemies(deltaTime);
		Grid.Rebuild(Enemies);
		float contactDamage = 0;
		foreach (Enemy enemy in Enemies)
		{
			enemy.SlowMultiplier = 1;
			if (!enemy.IsAlive) continue;
			if (PlayerCollider.Overlaps(enemy.Collider)) contactDamage = MathF.Max(contactDamage, enemy.ContactDamage);
		}
		// 같은 프레임의 접촉은 최대값 한 번만 적용합니다.
		ApplyDamage(Health, contactDamage);
		foreach (PendingTargetHit hit in _targetHits)
		{
			if (hit.Target.SpawnId == hit.SpawnId) ApplyDamage(hit.Target, hit.Damage);
			Grid.Query(hit.X, hit.Y, WeaponGeometry.LightningRadiusX + 64, _nearby);
			foreach (Enemy enemy in _nearby)
			{
				if (!enemy.IsAlive || enemy.SpawnId == hit.SpawnId) continue;
				if (WeaponGeometry.EllipseTouchesCircle(enemy.Transform.X - hit.X, enemy.Transform.Y - hit.Y, WeaponGeometry.LightningRadiusX, WeaponGeometry.LightningRadiusY, enemy.Collider.WorldRadius))
					ApplyDamage(enemy, hit.Damage);
			}
		}
		_targetHits.Clear();
		foreach (Attack attack in _attacks) if (attack.GameObject.ActiveSelf) attack.CheckHits();
		if (_fireAttack != null && _fireAttack.GameObject.ActiveSelf) _fireAttack.CheckHits();
	}
	private void SeparateEnemies(float deltaTime)
	{
		foreach (Enemy enemy in Enemies)
		{
			if (!enemy.IsAlive || enemy.Kind == EnemyKind.Bat) continue;
			Grid.Query(enemy.Collider.CenterX, enemy.Collider.CenterY, 70, _nearby);
			foreach (Enemy other in _nearby)
			{
				if (!other.IsAlive || other.Kind == EnemyKind.Bat || other.SpawnId <= enemy.SpawnId) continue;
				float dx = other.Collider.CenterX - enemy.Collider.CenterX, dy = other.Collider.CenterY - enemy.Collider.CenterY;
				float distance = MathF.Sqrt(dx * dx + dy * dy);
				float minimum = (enemy.Collider.WorldRadius + other.Collider.WorldRadius) * 0.85f;
				if (distance >= minimum) continue;
				if (distance < 0.01f) { dx = 1; dy = 0; distance = 1; }
				float push = MathF.Min((minimum - distance) * 0.5f, 50 * deltaTime);
				enemy.Transform.X -= dx / distance * push;
				enemy.Transform.Y -= dy / distance * push;
				other.Transform.X += dx / distance * push;
				other.Transform.Y += dy / distance * push;
			}
		}
	}
	public void QueueExperience(int value) { _experiencePending += value; }
	public void ApplyDamage(IDamageable target, float damage)
	{
		// 플레이어와 적은 각자의 무적 규칙을 내부에서 처리합니다.
		if (target.IsAlive && damage > 0) target.TakeDamage(damage);
	}
	public void ResolveDeaths()
	{
		foreach (Enemy enemy in Enemies)
		{
			if (!enemy.GameObject.ActiveSelf) continue;
			if (!enemy.IsAlive && !enemy.DeathProcessed)
			{
				enemy.BeginDeath();
				GameManager.Instance.RecordKill(enemy.Kind == EnemyKind.Boss);
				Audio.Play(enemy.Kind == EnemyKind.Boss ? "boss_death" : "enemy_death");
				if (enemy.Kind == EnemyKind.Boss) _bossPending++;
				else SpawnGem(enemy.Transform.X, enemy.Transform.Y - 12, enemy.Experience);
			}
			if (enemy.IsAlive && enemy.Kind != EnemyKind.Boss && CombatMath.DistanceSquared(enemy.Transform.X, enemy.Transform.Y, Player.Transform.X, Player.Transform.Y) > GameBalance.RemoveDistance * GameBalance.RemoveDistance)
				enemy.GameObject.SetActive(false);
		}
		if (!Health.IsAlive) { _experiencePending = 0; _bossPending = 0; return; }
		if (_experiencePending > 0) { Rewards.AddExperience(_experiencePending); Audio.Play("experience"); _experiencePending = 0; }
		Rewards.AddBossRewards(_bossPending);
		_bossPending = 0;
	}
	private void SpawnGem(float x, float y, int value)
	{
		ExperienceGem? gem = null;
		foreach (ExperienceGem candidate in _gems) if (!candidate.GameObject.ActiveSelf) { gem = candidate; break; }
		if (gem == null)
		{
			GameObject obj = new GameObject("ExperienceGem");
			SpriteRenderer renderer = obj.AddComponent(new SpriteRenderer(Assets.Frame("experience_gem"))); renderer.SortingLayer = 2;
			gem = obj.AddComponent(new ExperienceGem(this));
			_gems.Add(gem);
			_scene.AddGameObject(obj);
		}
		gem.Spawn(x, y, value);
	}
}

// 대상 선택은 공격 갱신에서, 실제 피해는 다른 충돌 피해와 같은 단계에서 적용합니다.
class PendingTargetHit
{
	public Enemy Target { get; }
	public int SpawnId { get; }
	public float Damage { get; }
	public float X { get; }
	public float Y { get; }
	public PendingTargetHit(Enemy target, float damage)
	{
		Target = target;
		SpawnId = target.SpawnId;
		Damage = damage;
		// 착탄 이펙트와 동일한 발 위치를 저장합니다. 번개 줄기는 충돌 검사하지 않습니다.
		X = target.Collider.CenterX;
		Y = target.Transform.Y;
	}
}
