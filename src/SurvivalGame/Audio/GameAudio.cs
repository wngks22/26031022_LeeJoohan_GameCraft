class GameAudio : IDisposable
{
	private readonly Dictionary<string, List<GameSound>> _sounds = new Dictionary<string, List<GameSound>>();
	private GameSound? _music;
	private GameSound? _fire;
	public float MusicVolume { get; set; } = 0.22f;
	public float EffectVolume { get; set; } = 0.35f;
	public void StartCombat()
	{
		if (G2AudioContext.Instance?.Audio == null) return;
		G2AudioContext.Instance.MasteringVoice?.SetVolume(1);
		_music = new GameSound("resource/audio/bgm/bgm_combat.wav");
		_music.SetVolume(MusicVolume); _music.Play(true);
		_fire = new GameSound("resource/audio/sfx/fire_area.wav");
		_fire.SetVolume(EffectVolume * 0.55f);
	}
	public void Play(string name)
	{
		if (G2AudioContext.Instance?.Audio == null) return;
		if (!_sounds.TryGetValue(name, out List<GameSound>? sounds))
		{
			sounds = new List<GameSound>(); _sounds.Add(name, sounds);
		}
		foreach (GameSound sound in sounds)
		{
			if (!sound.IsPlaying) { sound.SetVolume(EffectVolume); sound.Play(); return; }
		}
		// 같은 종류의 수백 건이 한 프레임에 겹치는 것만 제한합니다.
		if (sounds.Count >= 8) return;
		GameSound added = new GameSound("resource/audio/sfx/" + name + ".wav");
		sounds.Add(added); added.SetVolume(EffectVolume); added.Play();
	}
	public void UpdateState(bool paused, bool fireActive)
	{
		_music?.SetVolume(MusicVolume * (paused ? 0.35f : 1));
		if (_fire != null)
		{
			if (!fireActive || paused) _fire.Stop();
			else if (!_fire.IsPlaying) _fire.Play(true);
		}
		foreach (KeyValuePair<string, List<GameSound>> entry in _sounds)
		{
			// 선택 화면에서 들려야 하는 UI 효과음은 전투 정지와 분리합니다.
			if (entry.Key == "menu_move" || entry.Key == "menu_confirm" || entry.Key == "level_up") continue;
			foreach (GameSound sound in entry.Value) sound.Pause(paused);
		}
	}
	public void Dispose()
	{
		_music?.Dispose(); _music = null; _fire?.Dispose(); _fire = null;
		foreach (List<GameSound> sounds in _sounds.Values)
			foreach (GameSound sound in sounds) sound.Dispose();
		_sounds.Clear();
	}
}
