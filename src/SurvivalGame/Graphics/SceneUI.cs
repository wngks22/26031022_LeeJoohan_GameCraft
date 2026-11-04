using Vortice.Direct2D1;
using Vortice.Mathematics;

// 화면 좌표로 그리는 UI는 월드 카메라의 영향을 받지 않습니다.
class SceneUI
{
	private const int NumberWidth = 22;
	private const int TextHeight = 26;
	private const int ValueGap = 20;
	private readonly ResourceStore _resources;
	public SceneUI(ResourceStore resources)
	{
		_resources = resources;
	}

	public void Background(float opacity = 1)
	{
		_resources.GetTexture("resource/image/background/title_graveyard.png").Draw(0, 0, opacity, BitmapInterpolationMode.NearestNeighbor);
	}

	public void Logo()
	{
		_resources.GetTexture("resource/image/ui/title_logo.png").Draw(374, 112, 1, BitmapInterpolationMode.NearestNeighbor);
	}

	public bool IsHovered(G2InputContext input, float x, float y)
	{
		PointF mouse = input.MousePosition;
		bool insideX = mouse.X >= x - 110 && mouse.X <= x + 110;
		bool insideY = mouse.Y >= y - 24 && mouse.Y <= y + 24;
		return insideX && insideY;
	}

	public bool IsClicked(G2InputContext input, float x, float y)
	{
		return IsHovered(input, x, y) && input.IsButtonDown(MouseButtons.Left);
	}

	// 고정 문구는 미리 만든 PNG 한 장을 원본 크기로 출력합니다.
	public void TextImage(string name, int width, float x, float y, float opacity = 1)
	{
		G2Texture image = _resources.GetTexture("resource/image/ui/text/" + name + ".png");
		float left = x - width / 2;
		float top = y - TextHeight / 2;
		image.Draw(left, top, opacity, BitmapInterpolationMode.NearestNeighbor);
	}

	// numbers.png에는 0~9, 콜론이 각각 22x26 크기로 나란히 있습니다.
	public void Number(string text, float x, float y)
	{
		G2Texture image = _resources.GetTexture("resource/image/ui/text/numbers.png");
		int totalWidth = text.Length * NumberWidth;
		float left = x - totalWidth / 2;
		float top = y - TextHeight / 2;
		foreach (char character in text)
		{
			int index;
			if (character == ':') index = 10;
			// 숫자 문자는 순서대로 이어져 있어 '3' - '0'은 3이 됩니다.
			else if (character >= '0' && character <= '9') index = character - '0';
			else continue;
			Rect source = new Rect(index * NumberWidth, 0, NumberWidth, TextHeight);
			Rect destination = new Rect(left, top, NumberWidth, TextHeight);
			image.Draw(destination, source, 1, BitmapInterpolationMode.NearestNeighbor);
			left += NumberWidth;
		}
	}

	public void ResultValue(string name, int width, string value, float x, float y)
	{
		int numberWidth = value.Length * NumberWidth;
		int totalWidth = width + ValueGap + numberWidth;
		float left = x - totalWidth / 2;
		float labelCenter = left + width / 2;
		float numberCenter = left + width + ValueGap + numberWidth / 2;
		TextImage(name, width, labelCenter, y);
		Number(value, numberCenter, y);
	}

	public void Button(string name, int width, float x, float y, bool selected)
	{
		TextImage(name, width, x, y, selected ? 1 : 0.55f);
		if (selected)
		{
			_resources.GetTexture("resource/image/ui/menu_flame.png").Draw(x - width / 2 - 40, y - 18, 1, BitmapInterpolationMode.NearestNeighbor);
		}
	}
}
