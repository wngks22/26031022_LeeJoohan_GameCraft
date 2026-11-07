class GameplayScene : SceneBase
{
	private readonly G2InputContext _input;
	private readonly ResourceStore _resources;
	private readonly GameMain _game;
	private readonly SceneUI _ui;
	private GameAssets _assets = null!;
	private GameHUD _hud = null!;
	private GameAudio _audio = null!;
	private CombatWorld _world = null!;
	private EnemySpawner _spawner = null!;
	private bool _paused;
	private bool _ending;
	private float _endDelay;
	private int _selected;
	private PointF _previousMouse;
	public override SceneType Type => SceneType.Gameplay;
	public GameplayScene(GameMain game, ResourceStore resources, SceneUI ui)
	{
		_game = game; _input = game.Input; _ui = ui; _resources = resources;
	}
	public override void Enter()
	{
		base.Enter(); GameManager.Instance.Reset();
		_paused = false; _ending = false; _endDelay = 0; _selected = 0; _previousMouse = _input.MousePosition;
		_assets = new GameAssets(_resources); _hud = new GameHUD(_ui, _assets);
		_audio = new GameAudio(); _audio.StartCombat();
		G2Texture terrain = _resources.GetTexture("resource/image/background/terrain.png");
		GameObject background = new GameObject("Background"); background.AddComponent(new TiledBackground(terrain)); AddGameObject(background);
		GameObject decorations = new GameObject("Decorations"); decorations.AddComponent(new GroundDecorations(terrain)); AddGameObject(decorations);
		GameObject player = new GameObject("Player");
		SpriteRenderer renderer = player.AddComponent(new SpriteRenderer(_assets.Frame("player_move")));
		renderer.FlashTexture = _assets.Frame("player_move_hitflash").Texture;
		player.AddComponent(new PlayerController(_input));
		player.AddComponent(new Animator(renderer, _assets.Clip("player_move")));
		HitFlash flash = player.AddComponent(new HitFlash(renderer));
		player.AddComponent(new PlayerHealth(flash, _audio));
		player.AddComponent(new CircleCollider(17, -20));
		GameObject cameraObject = new GameObject("MainCamera");
		Camera camera = cameraObject.AddComponent(new Camera(player.Transform)); camera.OffsetY = -36; camera.SnapToTarget();
		MainCamera = camera; AddGameObject(cameraObject);
		_world = new CombatWorld(this, _assets, _audio, player); _spawner = new EnemySpawner(_world, camera);
		AddGameObject(player);
		ApplyPendingChanges();
	}
	public void PauseForFocusLoss()
	{
		if (_ending) return;
		_paused = true; _selected = 0; IsWorldPaused = true; _audio.UpdateState(true, false);
	}
	protected override void UpdateInput(float deltaTime)
	{
		if (_ending) return;
		if (_input.IsKeyDown(Keys.Escape)) { _paused = !_paused; _selected = 0; }
		if (_paused) UpdatePauseInput();
		else if (_world.Rewards.HasPending) UpdateRewardInput();
		_previousMouse = _input.MousePosition;
		IsWorldPaused = _paused || _world.Rewards.HasPending;
		_audio.UpdateState(IsWorldPaused, _world.Inventory.Find(WeaponKind.FireArea) != null);
	}
	private void UpdatePauseInput()
	{
		if (_input.IsKeyDown(Keys.Up) || _input.IsKeyDown(Keys.Down)) { _selected = 1 - _selected; _audio.Play("menu_move"); }
		for (int i = 0; i < 2; i++)
		{
			float y = 356 + i * 68;
			if (_input.MousePosition != _previousMouse && _ui.IsHovered(_input, 640, y)) _selected = i;
			if (_ui.IsClicked(_input, 640, y)) { ActivatePause(i); return; }
		}
		if (_input.IsKeyDown(Keys.Enter)) ActivatePause(_selected);
	}
	private void ActivatePause(int index)
	{
		_audio.Play("menu_confirm");
		if (index == 0) _paused = false; else _game.RequestScene(SceneType.Title);
		_selected = 0;
	}
	private void UpdateRewardInput()
	{
		_world.Rewards.PrepareOptions(); int count = _world.Rewards.Options.Count;
		if (_input.IsKeyDown(Keys.Left)) { _selected = (_selected + count - 1) % count; _audio.Play("menu_move"); }
		if (_input.IsKeyDown(Keys.Right)) { _selected = (_selected + 1) % count; _audio.Play("menu_move"); }
		for (int i = 0; i < count; i++)
		{
			if (_input.MousePosition != _previousMouse && _hud.IsChoiceHovered(_input, i, count)) _selected = i;
			if (_hud.IsChoiceHovered(_input, i, count) && _input.IsButtonDown(MouseButtons.Left)) { ChooseReward(i); return; }
		}
		if (_input.IsKeyDown(Keys.Enter)) ChooseReward(_selected);
	}
	private void ChooseReward(int index)
	{
		_world.Rewards.Choose(index); _selected = 0; _world.Rewards.PrepareOptions();
	}
	protected override void UpdateWorld(float deltaTime)
	{
		base.UpdateWorld(deltaTime); _world.Update(deltaTime); _spawner.Update(deltaTime);
	}
	protected override void UpdateCollisions(float deltaTime) { _world.CheckCollisions(deltaTime); }
	protected override void ResolveCombat()
	{
		_world.ResolveDeaths();
		if (!_world.Health.IsAlive) { _ending = true; IsWorldPaused = true; _audio.UpdateState(false, false); return; }
		if (_world.Rewards.HasPending)
		{
			_world.Rewards.PrepareOptions(); IsWorldPaused = true; _selected = 0; _audio.UpdateState(true, false);
		}
	}
	protected override void UpdateUI(float deltaTime)
	{
		if (!_ending) return;
		_endDelay += deltaTime;
		if (_endDelay >= 0.6f) _game.RequestScene(SceneType.Result);
	}
	public override void Render()
	{
		base.Render();
		if (MainCamera == null) return;
		_hud.Draw(_world, MainCamera);
		if (_ending) _hud.Fill(0, 0, 1280, 720, 0.12f, 0, 0, MathF.Min(0.6f, _endDelay));
		else if (_paused) _hud.DrawPause(_selected);
		else if (_world.Rewards.HasPending) _hud.DrawRewards(_world, _selected);
	}
	public override void Exit()
	{
		base.Exit(); _audio.Dispose();
		// 목록은 전투 관리 객체 안에 있으므로 씬의 참조도 함께 놓습니다.
		_world = null!; _spawner = null!;
	}
}
