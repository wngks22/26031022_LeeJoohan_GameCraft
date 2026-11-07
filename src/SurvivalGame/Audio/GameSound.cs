using Vortice.Multimedia;
using Vortice.XAudio2;

// 패키지와 같은 WAV 재생 방식에 개별 음량·일시 정지만 추가한 래퍼입니다.
class GameSound : IDisposable
{
	private readonly AudioBuffer _buffer;
	private readonly IXAudio2SourceVoice _voice;
	private bool _disposed;
	public bool IsPlaying => _voice.State.BuffersQueued > 0;
	public GameSound(string path)
	{
		using SoundStream stream = new SoundStream(File.OpenRead(G2Util.FindFilePath(path)));
		if (stream.Format == null) throw new InvalidDataException("WAV 형식이 없습니다: " + path);
		using var data = stream.ToDataStream();
		_buffer = new AudioBuffer(data);
		_voice = G2AudioContext.Instance!.Audio!.CreateSourceVoice(stream.Format);
	}
	public void SetVolume(float volume) { _voice.SetVolume(Math.Clamp(volume, 0, 1)); }
	public void Play(bool loop = false)
	{
		Stop();
		_buffer.LoopCount = loop ? (uint)XAudio2.LoopInfinite : 0;
		_voice.SubmitSourceBuffer(_buffer);
		_voice.Start();
	}
	public void Pause(bool paused) { if (paused) _voice.Stop(); else _voice.Start(); }
	public void Stop() { _voice.Stop(); _voice.FlushSourceBuffers(); }
	public void Dispose()
	{
		if (_disposed) return;
		_disposed = true;
		Stop(); _voice.Dispose(); _buffer.Dispose();
	}
}
