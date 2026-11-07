// 그림 제작 좌표를 기준으로 한 판정 규격입니다. 투명 여백·잔상은 공격 범위가 아닙니다.
static class WeaponGeometry
{
	// 손잡이 뒤와 날 중심을 잇고, 반지름으로 휘어진 날까지 감쌉니다.
	public static float ScytheBack(bool large) { return large ? 52 : 28; }
	public static float ScytheFront(bool large) { return large ? 280 : 148; }
	public static float ScytheFrontY(bool large) { return large ? 29 : 16; }
	public static float ScytheRadius(bool large) { return large ? 59 : 33; }
	public const float LightningRadiusX = 48;
	public const float LightningRadiusY = 28;

	// build_ground_fire.py: 바닥 타원은 (너비*0.5, 높이*0.59)에 그렸습니다.
	public static float FireCenterY(Sprite sprite, float worldY)
	{
		return worldY + sprite.Source.Height * (0.59f - (1 - sprite.PivotY));
	}
	public static float FireCenterX(Sprite sprite, float worldX)
	{
		return worldX + sprite.Source.Width * (0.5f - sprite.PivotX);
	}
	public static float FireRadiusX(Sprite sprite) { return sprite.Source.Width * 0.43f; }
	public static float FireRadiusY(Sprite sprite) { return sprite.Source.Height * 0.29f; }

	public static bool EllipseTouchesCircle(float dx, float dy, float radiusX, float radiusY, float circleRadius)
	{
		if (MathF.Abs(dx) > radiusX + circleRadius || MathF.Abs(dy) > radiusY + circleRadius) return false;
		if (dx * dx / (radiusX * radiusX) + dy * dy / (radiusY * radiusY) <= 1) return true;
		// 바깥에 있는 중심은 타원 테두리에 원이 닿는지 확인합니다.
		// 64개 짧은 선분으로 근사해, 두 반경에 원 반지름을 더할 때의 모서리 오차를 줄입니다.
		float previousX = radiusX, previousY = 0;
		for (int i = 1; i <= 64; i++)
		{
			float angle = i * MathF.Tau / 64;
			float x = MathF.Cos(angle) * radiusX, y = MathF.Sin(angle) * radiusY;
			if (CombatMath.SegmentHits(previousX, previousY, x, y, dx, dy, circleRadius)) return true;
			previousX = x; previousY = y;
		}
		return false;
	}
}
