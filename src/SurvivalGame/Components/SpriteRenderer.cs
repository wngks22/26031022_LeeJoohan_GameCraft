using System.Numerics;
using Vortice.Direct2D1;
using Vortice.Mathematics;

class SpriteRenderer : Renderer
{
	public Sprite Sprite { get; set; }
	public bool FlipX { get; set; }
	public bool FlipY { get; set; }
	public float Opacity { get; set; } = 1;
	public G2Texture? FlashTexture { get; set; }
	public float FlashAmount { get; set; }

	public SpriteRenderer(Sprite sprite)
	{
		Sprite = sprite;
		SortingLayer = 3;
	}

	public override void Render(Camera camera)
	{
		G2AppBase? app = G2AppBase.Instance;
		if (app == null) return;
		float x = MathF.Round(camera.WorldToScreenX(Transform.X));
		float y = MathF.Round(camera.WorldToScreenY(Transform.Y));
		float width = Sprite.Source.Width;
		float height = Sprite.Source.Height;
		float pivotX = width * Sprite.PivotX;
		float pivotY = height * (1 - Sprite.PivotY);
		Rect destination = new Rect(-pivotX, -pivotY, width, height);
		float scaleX = Transform.ScaleX;
		float scaleY = Transform.ScaleY;
		if (FlipX) scaleX = -scaleX;
		if (FlipY) scaleY = -scaleY;
		float radians = Transform.Rotation * MathF.PI / 180;
		Matrix3x2 previous = app.RenderTarget.Transform;
		Matrix3x2 scale = Matrix3x2.CreateScale(scaleX, scaleY);
		Matrix3x2 rotation = Matrix3x2.CreateRotation(radians);
		Matrix3x2 position = Matrix3x2.CreateTranslation(x, y);
		// 크기·반전 → 회전 → 화면 위치 → 기존 창 배율 순서로 적용합니다.
		Matrix3x2 drawingTransform = scale * rotation;
		drawingTransform = drawingTransform * position;
		drawingTransform = drawingTransform * previous;
		app.RenderTarget.Transform = drawingTransform;
		Sprite.Texture.Draw(destination, Sprite.Source, Opacity, BitmapInterpolationMode.NearestNeighbor);
		// 원본 직후에 그려 뒤쪽 적의 플래시가 앞쪽 적을 덮지 않게 합니다.
		if (FlashTexture != null && FlashAmount > 0)
			FlashTexture.Draw(destination, Sprite.Source, FlashAmount * Opacity, BitmapInterpolationMode.NearestNeighbor);
		// 다음 객체에는 이 스프라이트의 변환이 남지 않도록 복구합니다.
		app.RenderTarget.Transform = previous;
	}
}
