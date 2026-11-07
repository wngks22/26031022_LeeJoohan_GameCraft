using Vortice.Direct2D1;
using Vortice.Mathematics;

class TiledBackground : Renderer
{
	private readonly G2Texture _texture;
	private const int TileSize = 64;

	public TiledBackground(G2Texture texture)
	{
		_texture = texture;
	}

	public override void Render(Camera camera)
	{
		float left = camera.Transform.X - GameGlobal.ScreenSize.Width / 2.0f;
		float top = camera.Transform.Y - GameGlobal.ScreenSize.Height / 2.0f;
		// 음수 좌표에서도 같은 타일 격자를 유지하도록 내림합니다.
		float startX = MathF.Floor(left / TileSize) * TileSize;
		float startY = MathF.Floor(top / TileSize) * TileSize;
		Rect source = new Rect(0, 0, TileSize, TileSize);
		for (float y = startY; y < top + GameGlobal.ScreenSize.Height + TileSize; y += TileSize)
		{
			for (float x = startX; x < left + GameGlobal.ScreenSize.Width + TileSize; x += TileSize)
			{
				float screenX = MathF.Round(camera.WorldToScreenX(x));
				float screenY = MathF.Round(camera.WorldToScreenY(y));
				Rect destination = new Rect(screenX, screenY, TileSize, TileSize);
				_texture.Draw(destination, source, 1, BitmapInterpolationMode.NearestNeighbor);
			}
		}
	}
}
