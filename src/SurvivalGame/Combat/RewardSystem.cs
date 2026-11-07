enum RewardKind { Weapon, Heal, Damage }

class RewardOption
{
	public RewardKind Kind { get; }
	public WeaponKind Weapon { get; }
	public RewardOption(RewardKind kind, WeaponKind weapon = WeaponKind.MagicBolt) { Kind = kind; Weapon = weapon; }
}

class RewardSystem
{
	private readonly CombatWorld _world;
	private int _levelRewards;
	private int _bossRewards;
	public int Experience { get; private set; }
	public List<RewardOption> Options { get; } = new List<RewardOption>();
	public bool HasPending => _levelRewards > 0 || _bossRewards > 0;
	public bool IsBossReward => _levelRewards == 0 && _bossRewards > 0;
	public RewardSystem(CombatWorld world) { _world = world; }
	public void AddExperience(int amount)
	{
		Experience += amount;
		while (Experience >= GameBalance.RequiredExperience(GameManager.Instance.Level))
		{
			Experience -= GameBalance.RequiredExperience(GameManager.Instance.Level);
			GameManager.Instance.LevelUp();
			_levelRewards++;
		}
	}
	public void AddBossRewards(int count) { _bossRewards += count; }
	public void PrepareOptions()
	{
		if (!HasPending || Options.Count > 0) return;
		List<RewardOption> candidates = new List<RewardOption>();
		for (int i = 0; i < 4; i++)
		{
			WeaponKind kind = (WeaponKind)i;
			Weapon? weapon = _world.Inventory.Find(kind);
			bool canAcquire = weapon == null && _world.Inventory.Weapons.Count < GameBalance.WeaponSlots;
			bool canUpgrade = weapon != null && weapon.Level < GameBalance.MaxWeaponLevel;
			if (canAcquire || canUpgrade)
				candidates.Add(new RewardOption(RewardKind.Weapon, kind));
		}
		candidates.Add(new RewardOption(RewardKind.Heal));
		if (_world.Inventory.AllMaxed()) candidates.Add(new RewardOption(RewardKind.Damage));
		while (Options.Count < 3 && candidates.Count > 0)
		{
			int index = _world.Random.Next(candidates.Count);
			Options.Add(candidates[index]);
			candidates.RemoveAt(index);
		}
		_world.Audio.Play("level_up");
	}
	public void Choose(int index)
	{
		if (index < 0 || index >= Options.Count || !HasPending) return;
		RewardOption reward = Options[index];
		if (reward.Kind == RewardKind.Weapon) _world.Inventory.AddOrUpgrade(reward.Weapon);
		if (reward.Kind == RewardKind.Heal) _world.Health.Heal();
		if (reward.Kind == RewardKind.Damage) _world.Inventory.IncreaseDamage();
		if (_levelRewards > 0) _levelRewards--;
		else _bossRewards--;
		Options.Clear(); _world.Audio.Play("menu_confirm");
	}
}
