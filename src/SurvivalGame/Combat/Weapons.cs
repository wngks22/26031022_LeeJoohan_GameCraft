abstract class Weapon
{
	protected readonly CombatWorld World;
	private readonly List<WeaponStats> _levels;
	protected float CooldownRemaining;
	public WeaponKind Kind { get; }
	public int Level { get; private set; } = 1;
	public WeaponStats Stats => _levels[Level - 1];
	public Weapon(CombatWorld world, WeaponKind kind)
	{
		World = world;
		Kind = kind;
		_levels = GameBalance.CreateLevels(kind);
	}
	public void Upgrade() { if (Level < GameBalance.MaxWeaponLevel) Level++; }
	public abstract void Update(float deltaTime);
}

class ProjectileWeapon : Weapon
{
	private int _remaining;
	private float _delay;
	private float _angle;
	private WeaponStats _cast = null!;
	private float _damage;
	public ProjectileWeapon(CombatWorld world) : base(world, WeaponKind.MagicBolt) { }
	public override void Update(float deltaTime)
	{
		CooldownRemaining -= deltaTime;
		if (_remaining == 0 && CooldownRemaining <= 0)
		{
			Enemy? target = World.FindNearest(Stats.SearchRange);
			if (Stats.Aim == AimMode.NearestEnemy && target == null) return;
			_cast = Stats;
			_damage = _cast.Damage * World.Inventory.DamageMultiplier;
			if (_cast.Aim == AimMode.LastMovement)
				_angle = MathF.Atan2(World.Controller.DirectionY, World.Controller.DirectionX);
			else
				_angle = MathF.Atan2(target!.Collider.CenterY - World.PlayerCollider.CenterY, target.Collider.CenterX - World.PlayerCollider.CenterX);
			_remaining = _cast.Count;
			_delay = 0;
			CooldownRemaining = _cast.Cooldown;
		}
		_delay -= deltaTime;
		while (_remaining > 0 && _delay <= 0)
		{
			int index = _cast.Count - _remaining;
			float spread = (index - (_cast.Count - 1) / 2.0f) * _cast.Sweep * MathF.PI / 180;
			World.SpawnBolt(_angle + spread, _cast, _damage);
			_remaining--;
			_delay += _cast.Interval;
		}
	}
}

class AreaWeapon : Weapon
{
	private int _remaining;
	private float _delay;
	private bool _left;
	private WeaponStats _cast = null!;
	private float _damage;
	private int _castLevel;
	public AreaWeapon(CombatWorld world, WeaponKind kind) : base(world, kind) { }
	public override void Update(float deltaTime)
	{
		if (Kind == WeaponKind.FireArea) { World.UpdateFire(this); return; }
		CooldownRemaining -= deltaTime;
		if (_remaining == 0 && CooldownRemaining <= 0)
		{
			_cast = Stats;
			_castLevel = Level;
			_damage = _cast.Damage * World.Inventory.DamageMultiplier;
			_left = World.Controller.FacingLeft;
			_remaining = _cast.Count;
			_delay = 0;
			CooldownRemaining = _cast.Cooldown;
		}
		_delay -= deltaTime;
		while (_remaining > 0 && _delay <= 0)
		{
			World.SpawnSwing(_left, _castLevel, _cast, _damage);
			_remaining--;
			_delay += _cast.Interval;
		}
	}
}

class TargetWeapon : Weapon
{
	private readonly List<int> _selected = new List<int>();
	private int _remaining;
	private float _delay;
	private WeaponStats _cast = null!;
	private float _damage;
	public TargetWeapon(CombatWorld world) : base(world, WeaponKind.Lightning) { }
	public override void Update(float deltaTime)
	{
		CooldownRemaining -= deltaTime;
		if (_remaining == 0 && CooldownRemaining <= 0)
		{
			if (World.FindNearest(Stats.SearchRange, null, true) == null) return;
			_cast = Stats;
			_damage = _cast.Damage * World.Inventory.DamageMultiplier;
			_selected.Clear();
			_remaining = _cast.Count;
			_delay = 0;
			CooldownRemaining = _cast.Cooldown;
		}
		_delay -= deltaTime;
		while (_remaining > 0 && _delay <= 0)
		{
			Enemy? target;
			if (_cast.TargetSelection == TargetMode.Nearest) target = World.FindNearest(_cast.SearchRange, _selected, true);
			else target = World.FindRandom(_cast.SearchRange, _selected, true);
			if (target == null) { _remaining = 0; break; }
			if (!_cast.AllowDuplicateTargets) _selected.Add(target.SpawnId);
			World.SpawnLightning(target, _damage);
			_remaining--;
			_delay += _cast.Interval;
		}
	}
}

class WeaponInventory : Component
{
	private readonly CombatWorld _world;
	public List<Weapon> Weapons { get; } = new List<Weapon>();
	public float DamageMultiplier { get; private set; } = 1;
	public WeaponInventory(CombatWorld world) { _world = world; }
	public Weapon? Find(WeaponKind kind)
	{
		foreach (Weapon weapon in Weapons) if (weapon.Kind == kind) return weapon;
		return null;
	}
	public bool AddOrUpgrade(WeaponKind kind)
	{
		Weapon? existing = Find(kind);
		if (existing != null)
		{
			if (existing.Level >= GameBalance.MaxWeaponLevel) return false;
			existing.Upgrade();
			return true;
		}
		if (Weapons.Count >= GameBalance.WeaponSlots) return false;
		if (kind == WeaponKind.MagicBolt) Weapons.Add(new ProjectileWeapon(_world));
		else if (kind == WeaponKind.Lightning) Weapons.Add(new TargetWeapon(_world));
		else Weapons.Add(new AreaWeapon(_world, kind));
		return true;
	}
	public bool AllMaxed()
	{
		if (Weapons.Count != GameBalance.WeaponSlots) return false;
		foreach (Weapon weapon in Weapons) if (weapon.Level < GameBalance.MaxWeaponLevel) return false;
		return true;
	}
	public void IncreaseDamage() { DamageMultiplier += 0.05f; }
	// MonoBehaviour의 자동 Update가 아니라, 전투 갱신 순서에 맞춰 호출하는 함수입니다.
	public void UpdateWeapons(float deltaTime)
	{
		foreach (Weapon weapon in Weapons) weapon.Update(deltaTime);
	}
}
