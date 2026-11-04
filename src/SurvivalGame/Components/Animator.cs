class Animator : MonoBehaviour
{
	private readonly SpriteRenderer _renderer;
	private AnimationClip _clip;
	private float _elapsed;

	public int FrameIndex { get; private set; }
	public bool IsPlaying { get; set; }
	public bool IsFinished { get; private set; }
	public AnimationClip CurrentClip => _clip;

	public Animator(SpriteRenderer renderer, AnimationClip clip)
	{
		_renderer = renderer;
		_clip = clip;
		_renderer.Sprite = _clip.GetFrame(0);
	}

	// 클립 교체 시 새 클립의 첫 프레임부터 재생합니다.
	public void Play(AnimationClip clip)
	{
		_clip = clip;
		Restart();
	}

	// 풀에서 다시 꺼낼 때는 이전 프레임과 누적 시간을 비웁니다.
	public void Restart()
	{
		FrameIndex = 0;
		_elapsed = 0;
		IsFinished = false;
		IsPlaying = true;
		_renderer.Sprite = _clip.GetFrame(0);
	}

	public override void Update(float deltaTime)
	{
		// 단순 정지는 IsPlaying만 바꾸므로 현재 프레임과 시간을 보존합니다.
		if (!IsPlaying || IsFinished) return;
		_elapsed += deltaTime;
		while (_elapsed >= _clip.FrameSeconds)
		{
			_elapsed -= _clip.FrameSeconds;
			if (FrameIndex + 1 < _clip.FrameCount)
			{
				FrameIndex++;
			}
			else if (_clip.Loop)
			{
				FrameIndex = 0;
			}
			else
			{
				IsPlaying = false;
				IsFinished = true;
				_elapsed = 0;
				break;
			}
		}
		_renderer.Sprite = _clip.GetFrame(FrameIndex);
	}
}
