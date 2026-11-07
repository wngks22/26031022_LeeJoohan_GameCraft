// 가까운 적만 충돌 검사합니다. 딕셔너리는 월드의 96px 구역을 찾는 데 사용합니다.
class EnemyGrid
{
	private const int CellSize = 96;
	private readonly Dictionary<Point, List<Enemy>> _cells = new Dictionary<Point, List<Enemy>>();
	private readonly List<List<Enemy>> _freeLists = new List<List<Enemy>>();
	public void Rebuild(List<Enemy> enemies)
	{
		foreach (List<Enemy> cell in _cells.Values) { cell.Clear(); _freeLists.Add(cell); }
		_cells.Clear();
		foreach (Enemy enemy in enemies)
		{
			if (!enemy.IsAlive) continue;
			int x = (int)MathF.Floor(enemy.Collider.CenterX / CellSize);
			int y = (int)MathF.Floor(enemy.Collider.CenterY / CellSize);
			Point key = new Point(x, y);
			if (!_cells.TryGetValue(key, out List<Enemy>? cell))
			{
				if (_freeLists.Count == 0) cell = new List<Enemy>();
				else { cell = _freeLists[_freeLists.Count - 1]; _freeLists.RemoveAt(_freeLists.Count - 1); }
				_cells.Add(key, cell);
			}
			cell.Add(enemy);
		}
	}
	public void Query(float x, float y, float radius, List<Enemy> result)
	{
		result.Clear();
		int left = (int)MathF.Floor((x - radius) / CellSize), right = (int)MathF.Floor((x + radius) / CellSize);
		int top = (int)MathF.Floor((y - radius) / CellSize), bottom = (int)MathF.Floor((y + radius) / CellSize);
		for (int row = top; row <= bottom; row++)
			for (int column = left; column <= right; column++)
				if (_cells.TryGetValue(new Point(column, row), out List<Enemy>? cell)) result.AddRange(cell);
	}
}
