abstract class Behaviour : Component
{
	private bool _enabled = true;

	public bool Enabled
	{
		get { return _enabled; }
		set
		{
			if (_enabled == value) return;
			_enabled = value;
			if (GameObject != null) GameObject.RefreshActiveState();
		}
	}

}
