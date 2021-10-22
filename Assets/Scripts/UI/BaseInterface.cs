/// <summary>Base class for all interfaces</summary>
public class BaseInterface : BaseBehaviour
{
	public void Show()
	{
		if(!CheckInitialized())
			return;

		gameObject.SetActive(true);
	}

	public void Hide()
	{
		if(!CheckInitialized())
			return;

		gameObject.SetActive(false);
	}
}