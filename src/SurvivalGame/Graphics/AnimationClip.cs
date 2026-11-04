class AnimationClip
{
	private readonly List<Sprite> _frames = new List<Sprite>();

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
}
