using UnityEngine;

public class BaseBehaviour : MonoBehaviour
{
	public bool initialized => initInternal;
	protected string debugTag => "<b>[" + GetType() + "] : </b>";

	bool initInternal = false;

	protected void InitInternal()
	{
		initInternal = true;
		Debug.Log(debugTag + "Initialized");
	}

	protected bool CheckInitialized()
	{
		if(!enabled)
			return false;

		if(!initialized)
			Debug.LogError(debugTag + "Not initialized");

		return initialized;
	}

	void Update() { }
}