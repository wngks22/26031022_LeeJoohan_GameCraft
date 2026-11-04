// 타이틀과 결과 화면의 공통 메뉴 입력 및 BGM을 처리합니다.
abstract class MenuScene : SceneBase
{
	protected readonly GameMain Game;
	protected readonly SceneUI UI;
	protected int Selected;
	private readonly SceneMusic _music = new SceneMusic();
	private PointF _previousMouse;
	protected abstract float ButtonY { get; }

	protected MenuScene(GameMain game, SceneUI ui)
	{
		Game = game;
		UI = ui;
	}

	public override void Enter()
	{
		base.Enter();
		Selected = 0;
		_previousMouse = Game.Input.MousePosition;
		_music.Play("resource/audio/bgm/bgm_menu.wav");
	}

	protected override void UpdateInput(float deltaTime)
	{
		G2InputContext input = Game.Input;
		if (input.IsKeyDown(Keys.Up) || input.IsKeyDown(Keys.Down)) Selected = 1 - Selected;
		for (int i = 0; i < 2; i++)
		{
			float y = ButtonY + i * 68;
			if (input.MousePosition != _previousMouse && UI.IsHovered(input, 640, y)) Selected = i;
			if (UI.IsClicked(input, 640, y))
			{
				Activate(i);
				return;
			}
		}
		_previousMouse = input.MousePosition;
		if (input.IsKeyDown(Keys.Enter)) Activate(Selected);
	}

	protected abstract void Activate(int index);

	public override void Exit()
	{
		_music.Dispose();
		base.Exit();
	}
}
