class HitFlash : MonoBehaviour
{
	private readonly SpriteRenderer _renderer;
	private float _remaining;
	private float _duration;
	public HitFlash(SpriteRenderer renderer) { _renderer = renderer; }
	public void Play(float duration)
	{
		_duration = duration;
		_remaining = duration;
		_renderer.FlashAmount = 0.8f;
	}
	public void Clear() { _remaining = 0; _renderer.FlashAmount = 0; }
	public override void Update(float deltaTime)
	{
		_remaining = MathF.Max(0, _remaining - deltaTime);
		_renderer.FlashAmount = _duration > 0 ? 0.8f * _remaining / _duration : 0;
	}
}
