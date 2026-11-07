enum WeaponKind { MagicBolt, Scythe, Lightning, FireArea }
enum EnemyKind { Skeleton, Bat, Flower, Boss }
enum AimMode { NearestEnemy, LastMovement }
enum TargetMode { Random, Nearest }

// 테스트하면서 조정할 초기 수치입니다. 동작 코드와 분리합니다.
class WeaponStats
{
	public float Damage;
	public float Cooldown;
	public int Count;
	public float Interval;
	public float SearchRange;
	public float MaxDistance = 850;
	public float Lifetime = 4;
	public AimMode Aim = AimMode.NearestEnemy;
	public TargetMode TargetSelection = TargetMode.Random;
	public bool AllowDuplicateTargets = false;
	public float Speed;
	public float Sweep;
	public int MaxHits;
	public WeaponStats(float damage, float cooldown, int count, float interval, float searchRange, float speed, float sweep, int maxHits)
	{
		Damage = damage;
		Cooldown = cooldown;
		Count = count;
		Interval = interval;
		SearchRange = searchRange;
		Speed = speed;
		Sweep = sweep;
		MaxHits = maxHits;
	}
}

static class GameBalance
{
	public const int WeaponSlots = 3;
	public const int MaxWeaponLevel = 5;
	public const int EnemyLimit = 600;
	public const int AttackLimit = 180;
	public const float PlayerHealth = 100;
	public const float InvincibleSeconds = 0.8f;
	public const float EnemyFlashSeconds = 0.16f;
	public const float RemoveDistance = 1800;
	public const float GemLifetime = 70;
	public const float GemRemoveDistance = 1400;
	public const float PickupDistance = 78;
	public const float BossInterval = 90;
	public const float FirstFlowerTime = 210;
	public const float FlowerInterval = 90;
	public const float FirstBatTime = 75;
	public const float EnemySlow = 0.70f;

	public static List<WeaponStats> CreateLevels(WeaponKind kind)
	{
		List<WeaponStats> levels = new List<WeaponStats>();
		// 피해, 발동 간격, 개수, 순차 지연, 탐색 거리, 속도, 각도, 최대 적중 수.
		// 낫·화염의 실제 범위는 WeaponGeometry에서 시트와 함께 관리합니다.
		if (kind == WeaponKind.MagicBolt)
		{
			// 1~4레벨은 묶음 발사 뒤 쉬고, 5레벨만 끊김 없이 한 발씩 발사합니다.
			// 5레벨은 발사 빈도가 늘어나는 대신 한 발 피해를 낮춥니다.
			levels.Add(new WeaponStats(
				damage: 32, cooldown: 1.0f, count: 1, interval: 0.11f,
				searchRange: 650, speed: 420, sweep: 5, maxHits: 1));
			levels.Add(new WeaponStats(
				damage: 32, cooldown: 1.0f, count: 2, interval: 0.11f,
				searchRange: 650, speed: 420, sweep: 5, maxHits: 1));
			levels.Add(new WeaponStats(
				damage: 36, cooldown: 0.9f, count: 2, interval: 0.11f,
				searchRange: 650, speed: 420, sweep: 5, maxHits: 2));
			levels.Add(new WeaponStats(
				damage: 40, cooldown: 1.0f, count: 3, interval: 0.11f,
				searchRange: 650, speed: 420, sweep: 5, maxHits: 2));
			levels.Add(new WeaponStats(
				damage: 34, cooldown: 0.25f, count: 1, interval: 0,
				searchRange: 650, speed: 420, sweep: 0, maxHits: 2));
		}
		else if (kind == WeaponKind.Scythe)
		{
			levels.Add(new WeaponStats(
				damage: 40, cooldown: 2.6f, count: 1, interval: 0.2f,
				searchRange: 0, speed: 330, sweep: 120, maxHits: 0));
			levels.Add(new WeaponStats(
				damage: 50, cooldown: 2.6f, count: 1, interval: 0.2f,
				searchRange: 0, speed: 330, sweep: 150, maxHits: 0));
			levels.Add(new WeaponStats(
				damage: 60, cooldown: 2.6f, count: 2, interval: 0.2f,
				searchRange: 0, speed: 330, sweep: 180, maxHits: 0));
			levels.Add(new WeaponStats(
				damage: 75, cooldown: 2.6f, count: 2, interval: 0.2f,
				searchRange: 0, speed: 330, sweep: 210, maxHits: 0));
			levels.Add(new WeaponStats(
				damage: 110, cooldown: 2.6f, count: 3, interval: 0.2f,
				searchRange: 0, speed: 480, sweep: 360, maxHits: 0));
		}
		else if (kind == WeaponKind.Lightning)
		{
			levels.Add(new WeaponStats(
				damage: 100, cooldown: 3, count: 2, interval: 0.14f,
				searchRange: 600, speed: 0, sweep: 0, maxHits: 0));
			levels.Add(new WeaponStats(
				damage: 130, cooldown: 3, count: 3, interval: 0.14f,
				searchRange: 600, speed: 0, sweep: 0, maxHits: 0));
			levels.Add(new WeaponStats(
				damage: 170, cooldown: 3, count: 3, interval: 0.14f,
				searchRange: 600, speed: 0, sweep: 0, maxHits: 0));
			levels.Add(new WeaponStats(
				damage: 210, cooldown: 3, count: 4, interval: 0.14f,
				searchRange: 600, speed: 0, sweep: 0, maxHits: 0));
			levels.Add(new WeaponStats(
				damage: 300, cooldown: 3, count: 6, interval: 0.14f,
				searchRange: 600, speed: 0, sweep: 0, maxHits: 0));
		}
		else
		{
			levels.Add(new WeaponStats(
				damage: 8, cooldown: 0.7f, count: 1, interval: 0,
				searchRange: 0, speed: 0, sweep: 0, maxHits: 0));
			levels.Add(new WeaponStats(
				damage: 10, cooldown: 0.7f, count: 1, interval: 0,
				searchRange: 0, speed: 0, sweep: 0, maxHits: 0));
			levels.Add(new WeaponStats(
				damage: 13, cooldown: 0.7f, count: 1, interval: 0,
				searchRange: 0, speed: 0, sweep: 0, maxHits: 0));
			levels.Add(new WeaponStats(
				damage: 16, cooldown: 0.7f, count: 1, interval: 0,
				searchRange: 0, speed: 0, sweep: 0, maxHits: 0));
			levels.Add(new WeaponStats(
				damage: 24, cooldown: 0.6f, count: 1, interval: 0,
				searchRange: 0, speed: 0, sweep: 0, maxHits: 0));
		}
		return levels;
	}

	public static int RequiredExperience(int level)
	{
		int growth = Math.Max(0, level - 1);
		int lateGrowth = Math.Max(0, level - 15);
		// 초반에는 비용을 낮추고, 15레벨부터 기존 후반 비용으로 이어집니다.
		int required = 30 + growth * 10 + growth * growth * 2 + lateGrowth * lateGrowth * 6;
		float earlyRate = 0.6f + 0.4f * Math.Min(growth, 14) / 14;
		return (int)MathF.Round(required * earlyRate);
	}

	public static int NormalEnemyCount(float seconds)
	{
		if (seconds < 30) return 1;
		if (seconds < 120) return 2;
		// 생성 간격은 유지하고 60초마다 무리 수만 한 마리씩 늘립니다.
		return Math.Min(18, 3 + (int)((seconds - 120) / 60));
	}

	public static float NormalEnemyInterval(float seconds) { return 2.5f; }

	public static int BatCount(float seconds)
	{
		float elapsed = MathF.Max(0, seconds - FirstBatTime);
		return Math.Min(24, 8 + (int)(elapsed / 90) * 2);
	}
	public static float BatInterval(float seconds)
	{
		float elapsed = MathF.Max(0, seconds - FirstBatTime);
		return MathF.Max(30, 50 - elapsed / 30);
	}

	public static float Difficulty(float seconds)
	{
		// 첫 2분은 기본 체력으로 유지한 뒤 완만하게 증가합니다.
		float minutes = MathF.Floor(MathF.Max(0, seconds - 120) / 30) * 0.5f;
		return 1 + minutes * 0.25f + minutes * minutes * 0.04f;
	}

	public static float ContactDifficulty(float seconds)
	{
		// 체력 배율을 그대로 쓰면 갑자기 한 방에 사망하므로 피해 증가를 분리합니다.
		float minutes = MathF.Floor(seconds / 30) * 0.5f;
		return 1 + minutes * 0.16f;
	}
}
