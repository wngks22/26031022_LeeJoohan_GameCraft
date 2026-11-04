class ResultScene : MenuScene
{
	public override SceneType Type => SceneType.Result;
	protected override float ButtonY => 530;
	public ResultScene(GameMain game, SceneUI ui) : base(game, ui)
	{
	}

	protected override void Activate(int index)
	{
		if (index == 0) Game.RequestScene(SceneType.Gameplay);
		else Game.RequestScene(SceneType.Title);
	}

	public override void Render()
	{
		UI.Background(0.45f);
		UI.TextImage("result_heading", 108, 640, 140);
		UI.TextImage("survival_time", 108, 640, 234);
		UI.Number(GameManager.Instance.GetTimeText(), 640, 282);
		UI.ResultValue("final_level", 106, GameManager.Instance.Level.ToString(), 640, 350);
		UI.ResultValue("enemy_kills", 82, GameManager.Instance.Kills.ToString("000000"), 640, 396);
		UI.ResultValue("boss_kills", 106, GameManager.Instance.BossKills.ToString("000000"), 640, 442);
		UI.Button("restart", 74, 640, ButtonY, Selected == 0);
		UI.Button("title", 74, 640, ButtonY + 68, Selected == 1);
	}
}
