class SceneMusic : IDisposable
{
	private G2AudioSound? _sound;
	public void Play(string path)
	{
		Dispose();
		G2AudioContext? audio = G2AudioContext.Instance;
		if (audio == null || audio.Audio == null) return;
		_sound = new G2AudioSound(path);
		if (audio.MasteringVoice != null) audio.MasteringVoice.SetVolume(0.25f);
		_sound.Play(true);
	}

	public void Dispose()
	{
		// 재생을 멈춘 뒤 사운드가 사용하는 자원을 해제합니다.
		if (_sound == null) return;
		_sound.Stop();
		_sound.Dispose();
		_sound = null;
	}
}
