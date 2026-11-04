abstract class MonoBehaviour : Behaviour
{
	internal bool HasAwakened { get; set; }
	internal bool HasStarted { get; set; }
	internal bool IsRunning { get; set; }

	public virtual void Awake() { }
	public virtual void OnEnable() { }
	public virtual void Start() { }
	public virtual void Update(float deltaTime) { }
	public virtual void LateUpdate(float deltaTime) { }
	public virtual void OnDisable() { }
	public virtual void OnDestroy() { }
}
