using Vortice.Mathematics;

class Sprite
{
	public G2Texture Texture { get; }
	public Rect Source { get; }
	public float PivotX { get; }
	public float PivotY { get; }

	public Sprite(G2Texture texture, Rect source) : this(texture, source, 0.5f, 0.0f)
	{
	}

	public Sprite(G2Texture texture, Rect source, float pivotX, float pivotY)
	{
		Texture = texture;
		Source = source;
		PivotX = pivotX;
		PivotY = pivotY;
	}
}
