namespace GameFramework
{
	// System.Object와 구분하기 위해 공통 기반형만 이름 공간에 둡니다.
	abstract class Object
	{
		private static int _nextId = 1;

		public int Id { get; }
		public string Name { get; set; } = "";
		public bool IsDestroyed { get; internal set; }

		protected Object()
		{
			Id = _nextId;
			_nextId++;
		}
	}
}
