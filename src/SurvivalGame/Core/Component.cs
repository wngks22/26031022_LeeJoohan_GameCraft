abstract class Component : GameFramework.Object, IDisposable
{
	public GameObject GameObject { get; internal set; } = null!;
	public Transform Transform => GameObject.Transform;

	public virtual void Dispose()
	{
	}
}
