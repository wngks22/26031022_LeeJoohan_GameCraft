using Vortice.Mathematics;

class GameplayScene : SceneBase
{
	private readonly G2InputContext _input;
	private readonly ResourceStore _resources;
	private readonly GameMain _game;
	private readonly SceneUI _ui;
	private readonly SceneMusic _music = new SceneMusic();

	public override SceneType Type => SceneType.Gameplay;

	public GameplayScene(GameMain game, ResourceStore resources, SceneUI ui)
	{
		_game = game;
		_input = game.Input;
		_ui = ui;
		_resources = resources;
	}

	public override void Enter()
	{
		base.Enter();
		GameManager.Instance.Reset();
		G2Texture terrain = _resources.GetTexture("resource/image/background/terrain.png");
		G2Texture playerTexture = _resources.GetTexture("resource/image/player/player_move.png");

		GameObject background = new GameObject("Background");
		background.AddComponent(new TiledBackground(terrain));
		AddGameObject(background);

		List<Sprite> frames = new List<Sprite>();
		for (int i = 0; i < 4; i++)
		{
			frames.Add(new Sprite(playerTexture, new Rect(i * 66, 0, 66, 72)));
		}
		AnimationClip moveClip = new AnimationClip(frames, 0.24f, true);

		GameObject player = new GameObject("Player");
		SpriteRenderer renderer = player.AddComponent(new SpriteRenderer(frames[0]));
		// 이동 여부를 먼저 결정하고 같은 프레임에서 애니메이션을 갱신합니다.
		player.AddComponent(new PlayerController(_input));
		player.AddComponent(new Animator(renderer, moveClip));
		AddGameObject(player);

		GameObject cameraObject = new GameObject("MainCamera");
		Camera camera = cameraObject.AddComponent(new Camera(player.Transform));
		// Transform은 발 위치이므로 반 프레임 높이 위를 화면 중앙으로 추적합니다.
		camera.OffsetY = -36;
		camera.SnapToTarget();
		MainCamera = camera;
		AddGameObject(cameraObject);
		ApplyPendingChanges();

		_music.Play("resource/audio/bgm/bgm_combat.wav");
	}

	protected override void UpdateWorld(float deltaTime)
	{
		base.UpdateWorld(deltaTime);
		GameManager.Instance.AddPlayTime(deltaTime);
	}

	protected override void UpdateInput(float deltaTime)
	{
		if (_ui.IsClicked(_input, 1140, 40)) _game.RequestScene(SceneType.Result);
	}

	public override void Render()
	{
		base.Render();
		_ui.Number(GameManager.Instance.GetTimeText(), 640, 40);
		_ui.Button("exit", 50, 1140, 40, _ui.IsHovered(_input, 1140, 40));
	}

	public override void Exit()
	{
		base.Exit();
		_music.Dispose();
		// 공유 이미지는 GameMain의 ResourceStore가 프로그램 종료 시 해제합니다.
	}
}
