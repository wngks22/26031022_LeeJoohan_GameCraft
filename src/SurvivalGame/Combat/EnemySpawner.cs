class EnemySpawner
{
	private readonly CombatWorld _world;
	private readonly Camera _camera;
	private float _normal;
	private float _bat = GameBalance.FirstBatTime;
	private float _flower = GameBalance.FirstFlowerTime;
	private float _boss = GameBalance.BossInterval;
	public EnemySpawner(CombatWorld world, Camera camera) { _world = world; _camera = camera; }
	public void Update(float deltaTime)
	{
		_normal -= deltaTime; _bat -= deltaTime; _flower -= deltaTime; _boss -= deltaTime;
		if (_normal <= 0)
		{
			int count = GameBalance.NormalEnemyCount(_world.Time);
			for (int i = 0; i < count; i++)
			{
				float angle = _world.Random.NextSingle() * MathF.Tau;
				_world.SpawnEnemy(EnemyKind.Skeleton, _camera.Transform.X + MathF.Cos(angle) * 920, _camera.Transform.Y + MathF.Sin(angle) * 570);
			}
			_normal += GameBalance.NormalEnemyInterval(_world.Time);
		}
		if (_bat <= 0) { SpawnBats(); _bat += GameBalance.BatInterval(_world.Time); }
		if (_flower <= 0) { SpawnFlowers(); _flower += GameBalance.FlowerInterval; }
		if (_boss <= 0)
		{
			float angle = _world.Random.NextSingle() * MathF.Tau;
			Enemy? boss = _world.SpawnEnemy(EnemyKind.Boss, _camera.Transform.X + MathF.Cos(angle) * 950, _camera.Transform.Y + MathF.Sin(angle) * 590);
			if (boss != null) _world.Audio.Play("boss_spawn");
			_boss += GameBalance.BossInterval;
		}
	}
	private void SpawnBats()
	{
		float direction = _world.Random.Next(2) == 0 ? 1 : -1;
		float y = _world.Player.Transform.Y - 120 + _world.Random.NextSingle() * 240;
		int count = GameBalance.BatCount(_world.Time);
		for (int i = 0; i < count; i++)
		{
			int row = i % 4;
			int column = i / 4;
			Enemy? enemy = _world.SpawnEnemy(EnemyKind.Bat, _camera.Transform.X - direction * (760 + column * 36), y + (row - 1.5f) * 38);
			if (enemy is CrossingEnemy bat) { bat.DirectionX = direction; bat.DirectionY = 0; }
		}
	}
	private void SpawnFlowers()
	{
		// 카메라 모서리도 타원 안에 들어오게 하여 포위망 전체가 화면 밖에서 출발합니다.
		const float radiusX = 1000, radiusY = 600;
		int count = 170;
		for (int i = 0; i < count; i++)
		{
			float angle = i * MathF.Tau / count;
			_world.SpawnEnemy(EnemyKind.Flower, _camera.Transform.X + MathF.Cos(angle) * radiusX, _camera.Transform.Y + MathF.Sin(angle) * radiusY);
		}
	}
}
