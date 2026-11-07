class ExperienceGem : MonoBehaviour
{
	private readonly CombatWorld _world;
	private float _age;
	private bool _attracted;
	public int Value { get; private set; }
	public ExperienceGem(CombatWorld world) { _world = world; }
	public void Spawn(float x, float y, int value)
	{
		Transform.X = x; Transform.Y = y; Value = value;
		_age = 0; _attracted = false; GameObject.SetActive(true);
	}
	public override void Update(float deltaTime)
	{
		_age += deltaTime;
		float dx = _world.PlayerCollider.CenterX - Transform.X;
		float dy = _world.PlayerCollider.CenterY - Transform.Y;
		float distance = MathF.Sqrt(dx * dx + dy * dy);
		if (distance < GameBalance.PickupDistance) _attracted = true;
		if (_attracted)
		{
			if (distance <= MathF.Max(18, 360 * deltaTime))
			{
				_world.QueueExperience(Value); GameObject.SetActive(false); return;
			}
			Transform.X += dx / distance * 360 * deltaTime; Transform.Y += dy / distance * 360 * deltaTime;
		}
		if (_age > GameBalance.GemLifetime && distance > GameBalance.GemRemoveDistance) GameObject.SetActive(false);
	}
}
