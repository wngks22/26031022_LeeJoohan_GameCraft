// 화염의 추적·강화·판정을 담당합니다. 생성과 호출 순서는 CombatWorld가 관리합니다.
class FireAreaAttack : Attack
{
	private AreaWeapon _weapon = null!;
	private int _level;
	public FireAreaAttack(CombatWorld world) : base(world) { }

	// 모든 객체가 이동한 뒤 무기 갱신 단계에서 호출합니다. Update에서 중복 갱신하지 않습니다.
	public void Refresh(AreaWeapon weapon)
	{
		_weapon = weapon;
		if (_level != weapon.Level)
		{
			string clipName = weapon.Level == 1 ? "fire_area" : "fire_area_lv" + weapon.Level;
			Animator.Play(World.Assets.Clip(clipName));
			_level = weapon.Level;
		}
		Transform.X = World.Player.Transform.X;
		Transform.Y = World.Player.Transform.Y - WeaponGeometry.FireCenterY(Renderer.Sprite, 0);
	}
	public override void CheckHits()
	{
		WeaponStats stats = _weapon.Stats;
		Sprite sprite = Renderer.Sprite;
		float centerX = WeaponGeometry.FireCenterX(sprite, Renderer.Transform.X);
		float centerY = WeaponGeometry.FireCenterY(sprite, Renderer.Transform.Y);
		foreach (Enemy enemy in World.Enemies)
		{
			if (!enemy.IsAlive) continue;
			// 바닥 공격은 몸통 위가 아니라 적의 발 위치와 바닥 타원을 비교합니다.
			float dx = enemy.Transform.X - centerX;
			float dy = enemy.Transform.Y - centerY;
			if (!WeaponGeometry.EllipseTouchesCircle(dx, dy, WeaponGeometry.FireRadiusX(sprite), WeaponGeometry.FireRadiusY(sprite), enemy.Collider.WorldRadius)) continue;
			if (_weapon.Level == GameBalance.MaxWeaponLevel) enemy.SlowMultiplier = GameBalance.EnemySlow;
			if (World.Time < enemy.NextFireHitTime) continue;
			enemy.NextFireHitTime = World.Time + stats.Cooldown;
			World.ApplyDamage(enemy, stats.Damage * World.Inventory.DamageMultiplier);
		}
	}
}
