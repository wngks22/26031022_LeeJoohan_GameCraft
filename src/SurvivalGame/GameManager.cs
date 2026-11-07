// 현재 플레이의 기록입니다. 파일 저장은 하지 않습니다.
class GameManager
{
	// 프로그램에서 하나만 만들어 모든 씬이 공유합니다.
	private static readonly GameManager _instance = new GameManager();

	public static GameManager Instance
	{
		get { return _instance; }
	}

	// 외부에서 new GameManager()를 호출할 수 없게 합니다.
	private GameManager()
	{
	}

	public float PlaySeconds { get; private set; }
	public int Level { get; private set; } = 1;
	public int Kills { get; private set; }
	public int BossKills { get; private set; }
	public void LevelUp() { Level++; }
	public void RecordKill(bool boss)
	{
		Kills++;
		if (boss) BossKills++;
	}

	public void AddPlayTime(float deltaTime)
	{
		PlaySeconds += deltaTime;
	}

	public void Reset()
	{
		PlaySeconds = 0;
		Level = 1;
		Kills = 0;
		BossKills = 0;
	}

	public string GetTimeText()
	{
		int seconds = (int)PlaySeconds;
		return (seconds / 60).ToString("00") + ":" + (seconds % 60).ToString("00");
	}
}
