class AnimationClip
{
	private readonly List<Sprite> _frames = new List<Sprite>();
	private readonly List<float> _durations = new List<float>();

	public float FrameSeconds { get; }
	public bool Loop { get; }
	public int FrameCount => _frames.Count;

	public AnimationClip(List<Sprite> frames, float frameSeconds, bool loop)
	{
		if (frames.Count == 0 || frameSeconds <= 0)
		{
			throw new ArgumentException("프레임과 재생 시간은 0보다 커야 합니다.");
		}
		_frames.AddRange(frames);
		FrameSeconds = frameSeconds;
		Loop = loop;
	}

	public Sprite GetFrame(int index)
	{
		return _frames[index];
	}

	public AnimationClip(List<Sprite> frames, List<float> durations, bool loop)
		: this(frames, durations.Count > 0 ? durations[0] : 0, loop)
	{
		if (frames.Count != durations.Count) throw new ArgumentException("프레임별 시간이 필요합니다.");
		foreach (float duration in durations)
		{
			if (duration <= 0) throw new ArgumentException("재생 시간은 양수입니다.");
			_durations.Add(duration);
		}
	}

	public float GetDuration(int index)
	{
		if (_durations.Count == 0) return FrameSeconds;
		return _durations[index];
	}
}
