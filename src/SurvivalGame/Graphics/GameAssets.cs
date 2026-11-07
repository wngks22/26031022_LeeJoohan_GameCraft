using Vortice.Mathematics;

// 리소스 규격에 맞춰 한 번 구성한 클립을 여러 객체가 공유합니다.
class GameAssets
{
	private readonly ResourceStore _resources;
	private readonly Dictionary<string, AnimationClip> _clips = new Dictionary<string, AnimationClip>();
	public GameAssets(ResourceStore resources)
	{
		_resources = resources;
		Add("player_move", "image/player/player_move.png", 66, 72, 4, 0.500000000f, 0.000000000f, true, new List<float> { 0.240f, 0.240f, 0.240f, 0.240f });
		Add("skeleton_move", "image/enemy/skeleton_move.png", 60, 72, 4, 0.500000000f, 0.027777778f, true, new List<float> { 0.280f, 0.280f, 0.280f, 0.280f });
		Add("flower_red_move", "image/enemy/flower_red_move.png", 64, 68, 4, 0.500000000f, 0.029411765f, true, new List<float> { 0.280f, 0.280f, 0.280f, 0.280f });
		Add("flower_pink_move", "image/enemy/flower_pink_move.png", 64, 68, 4, 0.500000000f, 0.029411765f, true, new List<float> { 0.280f, 0.280f, 0.280f, 0.280f });
		Add("boss_move", "image/enemy/boss_move.png", 82, 90, 4, 0.500000000f, 0.000000000f, true, new List<float> { 0.240f, 0.240f, 0.240f, 0.240f });
		Add("boss_death", "image/enemy/boss_death.png", 128, 128, 14, 0.492187500f, -0.031250000f, false, new List<float> { 0.100f, 0.100f, 0.100f, 0.100f, 0.100f, 0.100f, 0.100f, 0.100f, 0.100f, 0.100f, 0.100f, 0.100f, 0.100f, 0.100f });
		Add("experience_gem", "image/pickup/experience_gem.png", 26, 32, 1, 0.500000000f, 0.500000000f, false, new List<float> { 0.100f });
		Add("bat_move", "image/enemy/bat_move.png", 54, 50, 4, 0.500000000f, 0.040000000f, true, new List<float> { 0.280f, 0.280f, 0.280f, 0.280f });
		Add("flower_red_death", "image/enemy/flower_red_death.png", 118, 88, 18, 0.406779661f, 0.000000000f, false, new List<float> { 0.070f, 0.070f, 0.070f, 0.070f, 0.070f, 0.070f, 0.070f, 0.070f, 0.070f, 0.070f, 0.070f, 0.070f, 0.070f, 0.070f, 0.070f, 0.070f, 0.070f, 0.070f });
		Add("flower_pink_death", "image/enemy/flower_pink_death.png", 108, 108, 16, 0.462962963f, 0.000000000f, false, new List<float> { 0.070f, 0.070f, 0.070f, 0.070f, 0.070f, 0.070f, 0.070f, 0.070f, 0.070f, 0.070f, 0.070f, 0.070f, 0.070f, 0.070f, 0.070f, 0.210f });
		Add("skeleton_death", "image/enemy/skeleton_death.png", 140, 112, 14, 0.500000000f, 0.017857143f, false, new List<float> { 0.060f, 0.060f, 0.060f, 0.060f, 0.060f, 0.060f, 0.060f, 0.060f, 0.060f, 0.060f, 0.060f, 0.060f, 0.060f, 0.060f });
		Add("bat_death", "image/enemy/bat_death.png", 134, 90, 14, 0.500000000f, 0.022222222f, false, new List<float> { 0.060f, 0.060f, 0.060f, 0.060f, 0.060f, 0.060f, 0.060f, 0.060f, 0.060f, 0.060f, 0.060f, 0.060f, 0.060f, 0.060f });
		Add("lightning", "image/effect/lightning.png", 200, 800, 4, 0.500000000f, 0.050000000f, false, new List<float> { 0.030f, 0.050f, 0.045f, 0.055f });
		Add("fire_area", "image/effect/fire_area_lv1.png", 192, 144, 12, 0.500000000f, 0.500000000f, true, new List<float> { 0.050f, 0.050f, 0.050f, 0.050f, 0.050f, 0.050f, 0.050f, 0.050f, 0.050f, 0.050f, 0.050f, 0.050f });
		Add("magic_bolt", "image/effect/magic_bolt.png", 56, 48, 4, 0.500000000f, 0.500000000f, true, new List<float> { 0.080f, 0.080f, 0.080f, 0.080f });
		Add("icon_magic_bolt", "image/ui/icon_magic_bolt.png", 48, 48, 1, 0.500000000f, 0.500000000f, false, new List<float> { 0.100f });
		Add("icon_scythe", "image/ui/icon_scythe.png", 48, 48, 1, 0.500000000f, 0.500000000f, false, new List<float> { 0.100f });
		Add("icon_lightning", "image/ui/icon_lightning.png", 48, 48, 1, 0.500000000f, 0.500000000f, false, new List<float> { 0.100f });
		Add("icon_fire_area", "image/ui/icon_fire_area.png", 48, 48, 1, 0.500000000f, 0.500000000f, false, new List<float> { 0.100f });
		Add("icon_heal", "image/ui/icon_heal.png", 48, 48, 1, 0.500000000f, 0.500000000f, false, new List<float> { 0.100f });
		Add("icon_damage_up", "image/ui/icon_damage_up.png", 48, 48, 1, 0.500000000f, 0.500000000f, false, new List<float> { 0.100f });
		Add("reward_panel", "image/ui/reward_card.png", 192, 288, 1, 0.000000000f, 1.000000000f, false, new List<float> { 0.100f });
		Add("weapon_slot", "image/ui/weapon_slot.png", 88, 96, 2, 0.000000000f, 1.000000000f, false, new List<float> { 0.100f, 0.100f });
		Add("health_fill", "image/ui/health_fill.png", 8, 8, 1, 0.000000000f, 1.000000000f, false, new List<float> { 0.100f });
		Add("experience_fill", "image/ui/experience_fill.png", 8, 8, 1, 0.000000000f, 1.000000000f, false, new List<float> { 0.100f });
		Add("bar_frame", "image/ui/bar_frame.png", 240, 16, 1, 0.000000000f, 1.000000000f, false, new List<float> { 0.100f });
		Add("scythe_small", "image/effect/scythe_small.png", 250, 396, 24, 0.176000000f, 0.500000000f, false, new List<float> { 0.014f, 0.014f, 0.014f, 0.014f, 0.014f, 0.014f, 0.014f, 0.014f, 0.014f, 0.014f, 0.014f, 0.014f, 0.014f, 0.014f, 0.014f, 0.014f, 0.014f, 0.014f, 0.014f, 0.014f, 0.014f, 0.014f, 0.014f, 0.014f });
		Add("scythe_large", "image/effect/scythe_large.png", 432, 712, 24, 0.157407407f, 0.500000000f, false, new List<float> { 0.014f, 0.014f, 0.014f, 0.014f, 0.014f, 0.014f, 0.014f, 0.014f, 0.014f, 0.014f, 0.014f, 0.014f, 0.014f, 0.014f, 0.014f, 0.014f, 0.014f, 0.014f, 0.014f, 0.014f, 0.014f, 0.014f, 0.014f, 0.014f });
		Add("fire_area_lv2", "image/effect/fire_area_lv2.png", 288, 216, 12, 0.500000000f, 0.500000000f, true, new List<float> { 0.050f, 0.050f, 0.050f, 0.050f, 0.050f, 0.050f, 0.050f, 0.050f, 0.050f, 0.050f, 0.050f, 0.050f });
		Add("fire_area_lv3", "image/effect/fire_area_lv3.png", 384, 288, 12, 0.500000000f, 0.500000000f, true, new List<float> { 0.050f, 0.050f, 0.050f, 0.050f, 0.050f, 0.050f, 0.050f, 0.050f, 0.050f, 0.050f, 0.050f, 0.050f });
		Add("fire_area_lv4", "image/effect/fire_area_lv4.png", 480, 360, 12, 0.500000000f, 0.500000000f, true, new List<float> { 0.050f, 0.050f, 0.050f, 0.050f, 0.050f, 0.050f, 0.050f, 0.050f, 0.050f, 0.050f, 0.050f, 0.050f });
		Add("fire_area_lv5", "image/effect/fire_area_lv5.png", 748, 562, 12, 0.500000000f, 0.500000000f, true, new List<float> { 0.050f, 0.050f, 0.050f, 0.050f, 0.050f, 0.050f, 0.050f, 0.050f, 0.050f, 0.050f, 0.050f, 0.050f });
		Add("player_move_hitflash", "image/hitflash/player_move.png", 66, 72, 4, 0.500000000f, 0.000000000f, true, new List<float> { 0.240f, 0.240f, 0.240f, 0.240f });
		Add("skeleton_move_hitflash", "image/hitflash/skeleton_move.png", 60, 72, 4, 0.500000000f, 0.027777778f, true, new List<float> { 0.280f, 0.280f, 0.280f, 0.280f });
		Add("bat_move_hitflash", "image/hitflash/bat_move.png", 54, 50, 4, 0.500000000f, 0.040000000f, true, new List<float> { 0.280f, 0.280f, 0.280f, 0.280f });
		Add("flower_red_move_hitflash", "image/hitflash/flower_red_move.png", 64, 68, 4, 0.500000000f, 0.029411765f, true, new List<float> { 0.280f, 0.280f, 0.280f, 0.280f });
		Add("flower_pink_move_hitflash", "image/hitflash/flower_pink_move.png", 64, 68, 4, 0.500000000f, 0.029411765f, true, new List<float> { 0.280f, 0.280f, 0.280f, 0.280f });
		Add("boss_move_hitflash", "image/hitflash/boss_move.png", 82, 90, 4, 0.500000000f, 0.000000000f, true, new List<float> { 0.240f, 0.240f, 0.240f, 0.240f });
	}

	private void Add(string name, string path, int width, int height, int count, float pivotX, float pivotY, bool loop, List<float> durations)
	{
		G2Texture texture = _resources.GetTexture("resource/" + path);
		List<Sprite> frames = new List<Sprite>();
		for (int i = 0; i < count; i++)
			frames.Add(new Sprite(texture, new Rect(i * width, 0, width, height), pivotX, pivotY));
		_clips.Add(name, new AnimationClip(frames, durations, loop));
	}

	public AnimationClip Clip(string name)
	{
		if (_clips.TryGetValue(name, out AnimationClip? clip)) return clip;
		throw new ArgumentException("없는 애니메이션: " + name);
	}

	public Sprite Frame(string name, int index = 0)
	{
		return Clip(name).GetFrame(index);
	}
}
