using Vortice.Mathematics;
using Vortice.Direct2D1;

class GameHUD
{
	private readonly SceneUI _ui;
	private readonly GameAssets _assets;
	public GameHUD(SceneUI ui, GameAssets assets) { _ui = ui; _assets = assets; }
	public static string WeaponIcon(WeaponKind kind)
	{
		if (kind == WeaponKind.MagicBolt) return "icon_magic_bolt";
		if (kind == WeaponKind.Scythe) return "icon_scythe";
		if (kind == WeaponKind.Lightning) return "icon_lightning";
		return "icon_fire_area";
	}
	public void DrawSprite(string name, float x, float y, int frame = 0)
	{
		Sprite sprite = _assets.Frame(name, frame);
		float w = sprite.Source.Width, h = sprite.Source.Height;
		sprite.Texture.Draw(new Rect(x - w / 2, y - h / 2, w, h), sprite.Source, 1, BitmapInterpolationMode.NearestNeighbor);
	}
	public void Fill(float x, float y, float w, float h, float r, float g, float b, float alpha = 1)
	{
		G2AppBase? app = G2AppBase.Instance;
		if (app == null) return;
		using var brush = app.RenderTarget.CreateSolidColorBrush(new Color4(r, g, b, alpha));
		app.RenderTarget.FillRectangle(new Rect(x, y, w, h), brush);
	}
	private void Bar(float x, float y, float width, float ratio, string fill)
	{
		Sprite frame = _assets.Frame("bar_frame"), color = _assets.Frame(fill);
		frame.Texture.Draw(new Rect(x, y, width, 16), frame.Source, 1, BitmapInterpolationMode.NearestNeighbor);
		float w = MathF.Floor((width - 8) * Math.Clamp(ratio, 0, 1));
		if (w > 0) color.Texture.Draw(new Rect(x + 4, y + 4, w, 8), color.Source, 1, BitmapInterpolationMode.NearestNeighbor);
	}
	public void Draw(CombatWorld world, Camera camera)
	{
		float x = camera.WorldToScreenX(world.Player.Transform.X), y = camera.WorldToScreenY(world.Player.Transform.Y);
		Bar(x - 32, y + 8, 64, world.Health.Current / world.Health.Maximum, "health_fill");
		Fill(0, 0, 1280, 48, 0.09f, 0.11f, 0.13f);
		_ui.TextImage("level", 40, 36, 24); _ui.Number(GameManager.Instance.Level.ToString(), 86, 24);
		Bar(124, 18, 1140, world.Rewards.Experience / (float)GameBalance.RequiredExperience(GameManager.Instance.Level), "experience_fill");
		_ui.Number(GameManager.Instance.GetTimeText(), 640, 76);
		_ui.TextImage("kills", 48, 1094, 76); _ui.Number(GameManager.Instance.Kills.ToString("000000"), 1198, 76);
		for (int i = 0; i < GameBalance.WeaponSlots; i++)
		{
			Weapon? weapon = i < world.Inventory.Weapons.Count ? world.Inventory.Weapons[i] : null;
			float slotX = 58 + i * 100;
			DrawSprite("weapon_slot", slotX, 108, weapon != null && weapon.Level == GameBalance.MaxWeaponLevel ? 1 : 0);
			if (weapon == null) continue;
			DrawSprite(WeaponIcon(weapon.Kind), slotX, 88);
			DrawWeaponLevel(weapon.Level, slotX, 134);
		}
	}
	private void DrawWeaponLevel(int level, float x, float y)
	{
		if (level >= GameBalance.MaxWeaponLevel)
		{
			_ui.TextImage("max_level", 56, x, y);
		}
		else
		{
			_ui.TextImage("level", 40, x - 12, y);
			_ui.Number(level.ToString(), x + 23, y);
		}
	}
	public float ChoiceX(int index, int count) { return 640 + (index - (count - 1) / 2.0f) * 232; }
	public bool IsChoiceHovered(G2InputContext input, int index, int count)
	{
		float x = ChoiceX(index, count);
		return input.MousePosition.X >= x - 96 && input.MousePosition.X <= x + 96 && input.MousePosition.Y >= 210 && input.MousePosition.Y <= 498;
	}
	public void DrawRewards(CombatWorld world, int selected)
	{
		Fill(0, 0, 1280, 720, 0, 0, 0, 0.72f);
		_ui.TextImage(world.Rewards.IsBossReward ? "boss_reward" : "reward_title", world.Rewards.IsBossReward ? 164 : 188, 640, 148);
		for (int i = 0; i < world.Rewards.Options.Count; i++)
		{
			float x = ChoiceX(i, world.Rewards.Options.Count);
			if (selected == i) Fill(x - 100, 206, 200, 296, 0.76f, 0.65f, 0.39f);
			DrawSprite("reward_panel", x, 354);
			RewardOption option = world.Rewards.Options[i];
			string icon = "icon_heal", title = "heal_name"; int width = 108;
			if (option.Kind == RewardKind.Weapon)
			{
				icon = WeaponIcon(option.Weapon);
				if (option.Weapon == WeaponKind.MagicBolt) { title = "magic_name"; width = 74; }
				if (option.Weapon == WeaponKind.Scythe) { title = "scythe_name"; width = 26; }
				if (option.Weapon == WeaponKind.Lightning) { title = "lightning_name"; width = 48; }
				if (option.Weapon == WeaponKind.FireArea) { title = "fire_name"; width = 106; }
				Weapon? weapon = world.Inventory.Find(option.Weapon);
				_ui.TextImage(weapon == null ? "acquire" : "upgrade", weapon == null ? 48 : 50, x, 362);
				DrawWeaponLevel(weapon == null ? 1 : weapon.Level + 1, x, 408);
			}
			else if (option.Kind == RewardKind.Heal) _ui.TextImage("heal_info", 116, x, 382);
			else { icon = "icon_damage_up"; title = "damage_name"; width = 132; _ui.TextImage("damage_info", 60, x, 382); }
			DrawSprite(icon, x, 268); _ui.TextImage(title, width, x, 316);
		}
		_ui.TextImage("choice_hint", 274, 640, 554);
	}
	public void DrawPause(int selected)
	{
		Fill(0, 0, 1280, 720, 0, 0, 0, 0.72f);
		_ui.TextImage("pause", 106, 640, 238);
		Fill(462, 300, 356, 2, 0.38f, 0.32f, 0.44f);
		_ui.Button("resume", 50, 640, 356, selected == 0); _ui.Button("title", 74, 640, 424, selected == 1);
		Fill(462, 480, 356, 2, 0.38f, 0.32f, 0.44f);
		_ui.TextImage("pause_hint", 150, 640, 536);
	}
}
