using System.Windows.Forms;

class PlayerController : MonoBehaviour
{
	private readonly G2InputContext _input;
	private SpriteRenderer _renderer = null!;
	private Animator _animator = null!;

	public float MoveSpeed { get; set; } = 180;
	public float DirectionX { get; private set; } = 1;
	public float DirectionY { get; private set; }
	public bool FacingLeft { get; private set; }

	public PlayerController(G2InputContext input)
	{
		_input = input;
	}

	public override void Awake()
	{
		// 다른 컴포넌트까지 모두 부착된 뒤 참조를 연결합니다.
		SpriteRenderer? renderer = GameObject.GetComponent<SpriteRenderer>();
		Animator? animator = GameObject.GetComponent<Animator>();
		if (renderer == null || animator == null)
		{
			throw new InvalidOperationException("플레이어에는 SpriteRenderer와 Animator가 필요합니다.");
		}
		_renderer = renderer;
		_animator = animator;
	}

	private bool IsHeld(Keys key)
	{
		return _input.IsKeyDown(key) || _input.IsKeyPress(key);
	}

	public override void Update(float deltaTime)
	{
		float directionX = 0;
		float directionY = 0;
		if (IsHeld(Keys.A) || IsHeld(Keys.Left)) directionX -= 1;
		if (IsHeld(Keys.D) || IsHeld(Keys.Right)) directionX += 1;
		if (IsHeld(Keys.W) || IsHeld(Keys.Up)) directionY -= 1;
		if (IsHeld(Keys.S) || IsHeld(Keys.Down)) directionY += 1;

		float length = MathF.Sqrt(directionX * directionX + directionY * directionY);
		_animator.IsPlaying = length > 0;
		if (length == 0) return;
		// 대각선도 같은 속도로 월드 좌표에서 이동합니다. 화면 경계 제한은 없습니다.
		directionX /= length;
		directionY /= length;
		DirectionX = directionX;
		DirectionY = directionY;
		if (directionX != 0) FacingLeft = directionX < 0;
		Transform.X += directionX * MoveSpeed * deltaTime;
		Transform.Y += directionY * MoveSpeed * deltaTime;
		if (directionX < 0) _renderer.FlipX = true;
		if (directionX > 0) _renderer.FlipX = false;
	}
}
