using Vortice.Mathematics;
using Vortice.Direct2D1;

// 구역 안의 위치는 무작위지만, 같은 판에서 다시 방문하면 같은 장식이 보입니다.
class GroundDecorations : Renderer
{
	private readonly G2Texture _terrain;
	private readonly int _seed = Random.Shared.Next();
	private const int CellWidth = 1600, CellHeight = 1400;
	public GroundDecorations(G2Texture terrain) { _terrain = terrain; }
	public override void Render(Camera camera)
	{
		int centerX = (int)MathF.Floor(camera.Transform.X / CellWidth);
		int centerY = (int)MathF.Floor(camera.Transform.Y / CellHeight);
		for (int row = centerY - 1; row <= centerY + 1; row++)
			for (int column = centerX - 1; column <= centerX + 1; column++)
			{
				Random random = new Random(HashCode.Combine(_seed, column, row));
				if (random.Next(4) == 0) continue;
				// 64px 장식을 포함해도 1280x720 화면에 최대 두 개만 들어오는 간격입니다.
				// 세로로는 한 구역, 가로로는 최대 두 구역의 장식이 동시에 보입니다.
				float x = MathF.Round(camera.WorldToScreenX(column * CellWidth + random.Next(1200)));
				float y = MathF.Round(camera.WorldToScreenY(row * CellHeight + random.Next(600)));
				if (x + 64 <= 0 || y + 64 <= 0 || x >= GameGlobal.ScreenSize.Width || y >= GameGlobal.ScreenSize.Height) continue;
				_terrain.Draw(new Rect(x, y, 64, 64), new Rect(random.Next(1, 7) * 64, 0, 64, 64), 1, BitmapInterpolationMode.NearestNeighbor);
			}
	}
}
