class Camera : MonoBehaviour
{
	public Transform Target { get; }
	public float FollowSpeed { get; set; } = 10;
	public float OffsetY { get; set; }

	public Camera(Transform target)
	{
		Target = target;
	}

	public void SnapToTarget()
	{
		Transform.X = Target.X;
		Transform.Y = Target.Y + OffsetY;
	}

	public override void LateUpdate(float deltaTime)
	{
		float targetX = Target.X;
		float targetY = Target.Y + OffsetY;
		float distanceX = targetX - Transform.X;
		float distanceY = targetY - Transform.Y;
		float followRatio = GetFollowRatio(deltaTime);

		Transform.X += distanceX * followRatio;
		Transform.Y += distanceY * followRatio;
	}

	private float GetFollowRatio(float deltaTime)
	{
		float speed = FollowSpeed;
		if (speed < 0) speed = 0;
		// Exp는 남겨 둘 거리의 비율을 계산합니다. 1에서 빼면 따라갈 비율입니다.
		// 기존 추적 느낌과 프레임 시간에 따른 계산을 유지합니다.
		float remainingRatio = MathF.Exp(-speed * deltaTime);
		return 1 - remainingRatio;
	}

	public float WorldToScreenX(float worldX)
	{
		return worldX - Transform.X + GameGlobal.ScreenSize.Width / 2.0f;
	}

	public float WorldToScreenY(float worldY)
	{
		return worldY - Transform.Y + GameGlobal.ScreenSize.Height / 2.0f;
	}
}
