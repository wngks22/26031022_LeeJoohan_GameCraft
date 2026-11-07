// -------------------------------------------------------------------------------------------------------------------------------------------------------------
// Author: 3dapi (https://github.com/3dapi)
// -------------------------------------------------------------------------------------------------------------------------------------------------------------

using Vortice.Mathematics;

class GameMain : G2AppBase
{
	private readonly List<SceneBase> _scenes = new List<SceneBase>();
	private SceneBase? _currentScene;
	private SceneType? _requestedScene;
	private readonly ResourceStore _resources = new ResourceStore();
	public GameAudio MenuAudio { get; } = new GameAudio();

	public override System.Drawing.Size ScreenSize => GameGlobal.ScreenSize;
	public override string GameName => GameGlobal.GameName;

	protected override void Initialize()
	{
		// OS 배율과 별개로 출력 좌표를 픽셀 기준으로 맞춥니다.
		RenderTarget.SetDpi(96, 96);
		ClearColor = new Color4(0.06f, 0.08f, 0.1f, 1.0f);
		SceneUI ui = new SceneUI(_resources);
		_scenes.Add(new TitleScene(this, ui));
		_scenes.Add(new GameplayScene(this, _resources, ui));
		_scenes.Add(new ResultScene(this, ui));
		RequestScene(SceneType.Title);
		ApplySceneChange();
	}

	protected override void Update()
	{
		if (_currentScene == null) return;
		if (System.Windows.Forms.Form.ActiveForm == null)
		{
			if (_currentScene is GameplayScene gameplay) gameplay.PauseForFocusLoss();
			return;
		}
		// 창 이동이나 디버깅 뒤 큰 시간 차이로 한 번에 멀리 이동하지 않게 합니다.
		float deltaTime = (float)Math.Min(DeltaTime, 0.05);
		_currentScene.Update(deltaTime);
		_currentScene.LateUpdate(deltaTime);
		ApplySceneChange();
	}

	public void RequestScene(SceneType type)
	{
		_requestedScene = type;
	}

	private void ApplySceneChange()
	{
		if (_requestedScene == null) return;
		SceneBase? nextScene = null;
		foreach (SceneBase scene in _scenes)
		{
			if (scene.Type == _requestedScene.Value)
			{
				nextScene = scene;
				break;
			}
		}
		if (nextScene == null) throw new InvalidOperationException("등록되지 않은 씬입니다.");
		_requestedScene = null;
		if (_currentScene != null) _currentScene.Exit();
		_currentScene = nextScene;
		_currentScene.Enter();
	}

	protected override void Render()
	{
		if (_currentScene != null)
		{
			_currentScene.Render();
		}
	}

	public override void Dispose()
	{
		if (_currentScene != null)
		{
			_currentScene.Exit();
			_currentScene = null;
		}
		_scenes.Clear();
		MenuAudio.Dispose();
		_resources.Dispose();
		// 게임 리소스를 먼저 정리한 뒤 패키지의 그래픽·오디오 장치를 해제합니다.
		base.Dispose();
	}
}
