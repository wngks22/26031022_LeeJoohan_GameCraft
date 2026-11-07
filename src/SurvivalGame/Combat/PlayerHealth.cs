interface IDamageable
{
	bool IsAlive { get; }
	void TakeDamage(float damage);
}

class PlayerHealth : MonoBehaviour, IDamageable
{
	private readonly HitFlash _flash;
	private readonly GameAudio _audio;
	private float _invincible;
	public float Maximum { get; } = GameBalance.PlayerHealth;
	public float Current { get; private set; } = GameBalance.PlayerHealth;
	public bool IsAlive => Current > 0;
	public PlayerHealth(HitFlash flash, GameAudio audio) { _flash = flash; _audio = audio; }
	public override void Update(float deltaTime) { _invincible = MathF.Max(0, _invincible - deltaTime); }
	public void TakeDamage(float damage)
	{
		if (!IsAlive || _invincible > 0 || damage <= 0) return;
		Current = MathF.Max(0, Current - damage);
		_invincible = GameBalance.InvincibleSeconds;
		_flash.Play(_invincible);
		_audio.Play(IsAlive ? "player_hit" : "player_death");
	}
	public void Heal() { if (IsAlive) Current = MathF.Min(Maximum, Current + Maximum * 0.2f); }
}
