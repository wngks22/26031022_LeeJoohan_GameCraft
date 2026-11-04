class TitleScene : MenuScene
{
	public override SceneType Type => SceneType.Title;
	protected override float ButtonY => 398;
	public TitleScene(GameMain game, SceneUI ui) : base(game, ui) { }

	protected override void Activate(int index)
	{
		if (index == 0) Game.RequestScene(SceneType.Gameplay);
		else Game.Close();
	}

	public override void Render()
	{
		UI.Background();
		UI.Logo();
		UI.Button("game_start", 108, 640, ButtonY, Selected == 0);
		UI.Button("exit", 50, 640, ButtonY + 68, Selected == 1);
		UI.TextImage("move_hint", 376, 640, 620);
	}
}
