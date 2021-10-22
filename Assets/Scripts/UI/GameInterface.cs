using TMPro;
using UnityEngine;

/// <summary>Manages what is displayed on the interface during the game phase</summary>
public class GameInterface : BaseInterface
{
	[Header("Scene references")]
	public TextMeshProUGUI scoreDisplay;

	public void Init()
	{
		InitInternal();
	}

	public void UpdateScore(float value)
	{
		if(!CheckInitialized())
			return;

		scoreDisplay.text = Mathf.RoundToInt(value).ToString();
	}
}