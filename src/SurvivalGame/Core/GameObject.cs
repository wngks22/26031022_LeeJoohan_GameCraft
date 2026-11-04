class GameObject : GameFramework.Object, IDisposable
{
	private readonly List<Component> _components = new List<Component>();
	private bool _initialized;
	private bool _disposed;

	public Transform Transform { get; }
	public SceneBase? Scene { get; internal set; }
	public bool ActiveSelf { get; private set; } = true;

	public GameObject(string name)
	{
		Name = name;
		Transform = AddComponent<Transform>();
	}

	public T AddComponent<T>() where T : Component, new()
	{
		return AddComponent(new T());
	}

	public T AddComponent<T>(T component) where T : Component
	{
		if (Scene != null || IsDestroyed)
		{
			throw new InvalidOperationException("컴포넌트는 씬 등록 전에 구성합니다.");
		}
		foreach (Component existing in _components)
		{
			if (existing.GetType() == component.GetType())
			{
				throw new InvalidOperationException("같은 컴포넌트를 두 번 부착할 수 없습니다.");
			}
		}
		if (component.GameObject != null || component.IsDestroyed)
		{
			throw new InvalidOperationException("부착할 수 없는 컴포넌트입니다.");
		}
		component.GameObject = this;
		_components.Add(component);
		return component;
	}

	public T? GetComponent<T>() where T : Component
	{
		foreach (Component component in _components)
		{
			if (component is T matched) return matched;
		}
		return null;
	}

	internal void Initialize()
	{
		if (_initialized || IsDestroyed) return;
		_initialized = true;
		foreach (Component component in _components)
		{
			if (IsDestroyed) break;
			if (component is MonoBehaviour behaviour)
			{
				behaviour.HasAwakened = true;
				behaviour.Awake();
			}
		}
		RefreshActiveState();
	}

	public void SetActive(bool active)
	{
		if (IsDestroyed || ActiveSelf == active) return;
		ActiveSelf = active;
		RefreshActiveState();
	}

	internal void RefreshActiveState()
	{
		if (!_initialized) return;
		foreach (Component component in _components)
		{
			if (component is MonoBehaviour behaviour && behaviour.HasAwakened)
			{
				bool running = ActiveSelf && behaviour.Enabled && !IsDestroyed;
				if (running != behaviour.IsRunning)
				{
					behaviour.IsRunning = running;
					if (running) behaviour.OnEnable();
					else behaviour.OnDisable();
				}
			}
		}
	}

	internal void Update(float deltaTime)
	{
		if (!ActiveSelf || IsDestroyed) return;
		foreach (Component component in _components)
		{
			if (component is MonoBehaviour behaviour && behaviour.IsRunning)
			{
				if (!behaviour.HasStarted)
				{
					behaviour.HasStarted = true;
					behaviour.Start();
				}
				if (behaviour.IsRunning) behaviour.Update(deltaTime);
			}
		}
	}

	internal void LateUpdate(float deltaTime)
	{
		if (!ActiveSelf || IsDestroyed) return;
		foreach (Component component in _components)
		{
			if (component is MonoBehaviour behaviour && behaviour.IsRunning && behaviour.HasStarted)
			{
				behaviour.LateUpdate(deltaTime);
			}
		}
	}

	internal void Render(Camera camera)
	{
		if (!ActiveSelf || IsDestroyed) return;
		foreach (Component component in _components)
		{
			if (component is Behaviour behaviour && behaviour.Enabled)
			{
				behaviour.Render(camera);
			}
		}
	}

	public void Destroy()
	{
		// 씬 순회 중이라면 즉시 해제하지 않고 제거를 예약합니다.
		if (Scene != null) Scene.RemoveGameObject(this);
		else Dispose();
	}

	public void Dispose()
	{
		// 실제 정리는 한 번만 수행하며, 소유한 컴포넌트도 함께 정리합니다.
		if (_disposed) return;
		_disposed = true;
		IsDestroyed = true;
		RefreshActiveState();
		foreach (Component component in _components)
		{
			if (component is MonoBehaviour behaviour && behaviour.HasAwakened)
			{
				behaviour.OnDestroy();
			}
			component.IsDestroyed = true;
			component.Dispose();
		}
		_components.Clear();
		Scene = null;
	}
}
