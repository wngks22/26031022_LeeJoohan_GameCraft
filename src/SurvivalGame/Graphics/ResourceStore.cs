class ResourceStore : IDisposable
{
	private readonly Dictionary<string, G2Texture> _textures = new Dictionary<string, G2Texture>();

	public G2Texture GetTexture(string path)
	{
		if (_textures.TryGetValue(path, out G2Texture? texture))
		{
			return texture;
		}
		texture = new G2Texture(path);
		_textures.Add(path, texture);
		return texture;
	}

	public void Dispose()
	{
		// 공유 이미지는 개별 객체가 아닌 저장소가 프로그램 종료 때 해제합니다.
		foreach (G2Texture texture in _textures.Values)
		{
			texture.Dispose();
		}
		_textures.Clear();
	}
}
