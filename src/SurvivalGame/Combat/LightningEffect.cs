// 피해 판정 없이 재생만 담당하고, 재생이 끝나면 재사용 대기 상태로 돌아갑니다.
class LightningEffect : MonoBehaviour
{
	private readonly AnimationClip _clip;
	public SpriteRenderer Renderer { get; set; } = null!;
	public Animator Animator { get; set; } = null!;
	public LightningEffect(AnimationClip clip) { _clip = clip; }
	public void Show(float x, float y)
	{
		Transform.X = x;
		Transform.Y = y;
		Transform.Rotation = 0;
		Renderer.FlipX = false;
		Renderer.Opacity = 1;
		GameObject.SetActive(true);
		Animator.Play(_clip);
	}
	public override void Update(float deltaTime)
	{
		if (Animator.IsFinished) GameObject.SetActive(false);
	}
}
