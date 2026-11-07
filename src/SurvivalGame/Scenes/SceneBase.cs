abstract class SceneBase
{
	private readonly List<GameObject> _objects = new List<GameObject>();
	private readonly List<GameObject> _pendingAdd = new List<GameObject>();
	private readonly List<GameObject> _pendingRemove = new List<GameObject>();
	private bool _isExiting;
	private bool _worldUpdated;
	private readonly List<GameObject> _drawing = new List<GameObject>();

	public Camera? MainCamera { get; protected set; }
	public int ObjectCount => _objects.Count;
	public bool IsWorldPaused { get; protected set; }
	public abstract SceneType Type { get; }
	public virtual void Enter()
	{
		_isExiting = false;
		_worldUpdated = false;
		IsWorldPaused = false;
	}

	public void AddGameObject(GameObject gameObject)
	{
		if (gameObject.Scene != null || gameObject.IsDestroyed)
		{
			throw new InvalidOperationException("이미 소속이 있거나 제거된 객체입니다.");
		}
		// 종료 콜백에서 생성한 객체는 씬에 넣지 않고 바로 정리합니다.
		if (_isExiting)
		{
			gameObject.Dispose();
			return;
		}
		gameObject.Scene = this;
		_pendingAdd.Add(gameObject);
	}

	public void RemoveGameObject(GameObject gameObject)
	{
		if (gameObject.Scene != this || gameObject.IsDestroyed) return;
		gameObject.IsDestroyed = true;
		gameObject.RefreshActiveState();
		if (!_isExiting) _pendingRemove.Add(gameObject);
	}

	// 초기 구성 직후 또는 순회가 끝난 시점에만 호출합니다.
	internal void ApplyPendingChanges()
	{
		if (_isExiting) return;
		List<GameObject> removing = new List<GameObject>(_pendingRemove);
		_pendingRemove.Clear();
		foreach (GameObject gameObject in removing)
		{
			_pendingAdd.Remove(gameObject);
			_objects.Remove(gameObject);
			gameObject.Dispose();
		}
		List<GameObject> adding = new List<GameObject>(_pendingAdd);
		_pendingAdd.Clear();
		foreach (GameObject gameObject in adding)
		{
			if (gameObject.IsDestroyed) continue;
			_objects.Add(gameObject);
			gameObject.Initialize();
		}
	}

	// 전체 순서는 고정하고, 파생 씬은 아래 단계별 함수를 재정의합니다.
	public void Update(float deltaTime)
	{
		_worldUpdated = false;
		if (_isExiting) return;
		UpdateInput(deltaTime);
		ApplyPendingChanges();
		if (!IsWorldPaused)
		{
			UpdateWorld(deltaTime);
			UpdateCollisions(deltaTime);
			ResolveCombat();
			_worldUpdated = true;
		}
		ApplyPendingChanges();
		UpdateUI(deltaTime);
	}

	protected virtual void UpdateInput(float deltaTime) { }

	protected virtual void UpdateWorld(float deltaTime)
	{
		foreach (GameObject gameObject in _objects) gameObject.Update(deltaTime);
	}

	// 충돌 판정과, 모아 둔 피해·사망·보상의 확정을 별도 단계로 처리합니다.
	protected virtual void UpdateCollisions(float deltaTime) { }
	protected virtual void ResolveCombat() { }
	protected virtual void UpdateUI(float deltaTime) { }

	public void LateUpdate(float deltaTime)
	{
		if (_isExiting || IsWorldPaused || !_worldUpdated) return;
		foreach (GameObject gameObject in _objects) gameObject.LateUpdate(deltaTime);
	}

	public virtual void Render()
	{
		if (MainCamera == null) return;
		_drawing.Clear();
		foreach (GameObject gameObject in _objects)
		{
			if (gameObject.ActiveSelf && !gameObject.IsDestroyed) _drawing.Add(gameObject);
		}
		_drawing.Sort(CompareDrawing);
		foreach (GameObject gameObject in _drawing) gameObject.Render(MainCamera);
	}

	private int CompareDrawing(GameObject a, GameObject b)
	{
		Renderer? first = a.GetRenderer();
		Renderer? second = b.GetRenderer();
		int layerA = first == null ? 0 : first.SortingLayer;
		int layerB = second == null ? 0 : second.SortingLayer;
		int result = layerA.CompareTo(layerB);
		if (result != 0) return result;
		int orderA = first == null ? 0 : first.Order;
		int orderB = second == null ? 0 : second.Order;
		result = orderA.CompareTo(orderB);
		if (result != 0) return result;
		if (layerA == 3)
		{
			result = a.Transform.Y.CompareTo(b.Transform.Y);
			if (result != 0) return result;
		}
		return a.Id.CompareTo(b.Id);
	}

	public virtual void Exit()
	{
		if (_isExiting) return;
		_isExiting = true;
		_worldUpdated = false;
		// 콜백이 예약 목록을 바꿔도 정리 중인 목록에는 영향을 주지 않습니다.
		List<GameObject> disposing = new List<GameObject>(_objects);
		disposing.AddRange(_pendingAdd);
		_objects.Clear();
		_drawing.Clear();
		_pendingAdd.Clear();
		_pendingRemove.Clear();
		MainCamera = null;
		foreach (GameObject gameObject in disposing) gameObject.Dispose();
	}
}
