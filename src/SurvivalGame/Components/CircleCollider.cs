// 충돌 판정은 이미지 크기와 독립적이며 Transform의 배율·회전을 따릅니다.
class CircleCollider : Component
{
	public float Radius { get; set; }
	public float OffsetX { get; set; }
	public float OffsetY { get; set; }
	public float WorldRadius => Radius * MathF.Max(MathF.Abs(Transform.ScaleX), MathF.Abs(Transform.ScaleY));
	public float CenterX
	{
		get
		{
			float angle = Transform.Rotation * MathF.PI / 180;
			return Transform.X + OffsetX * Transform.ScaleX * MathF.Cos(angle) - OffsetY * Transform.ScaleY * MathF.Sin(angle);
		}
	}
	public float CenterY
	{
		get
		{
			float angle = Transform.Rotation * MathF.PI / 180;
			return Transform.Y + OffsetX * Transform.ScaleX * MathF.Sin(angle) + OffsetY * Transform.ScaleY * MathF.Cos(angle);
		}
	}
	public CircleCollider(float radius, float offsetY = 0) { Radius = radius; OffsetY = offsetY; }
	public bool Overlaps(CircleCollider other)
	{
		return CombatMath.DistanceSquared(CenterX, CenterY, other.CenterX, other.CenterY) <= MathF.Pow(WorldRadius + other.WorldRadius, 2);
	}
}

static class CombatMath
{
	public static float DistanceSquared(float x, float y, float otherX, float otherY)
	{
		float dx = otherX - x, dy = otherY - y;
		return dx * dx + dy * dy;
	}

	// 빠른 투사체가 한 프레임 사이에 적을 지나쳐도 선분으로 검사합니다.
	public static bool SegmentHits(float ax, float ay, float bx, float by, float x, float y, float radius)
	{
		float dx = bx - ax, dy = by - ay;
		float lengthSquared = dx * dx + dy * dy;
		float t = 0;
		if (lengthSquared > 0) t = Math.Clamp(((x - ax) * dx + (y - ay) * dy) / lengthSquared, 0, 1);
		return DistanceSquared(ax + dx * t, ay + dy * t, x, y) <= radius * radius;
	}
}
