using TMPro;
using UnityEngine;

/// <summary>Manages what is displayed on the interface during the game phase</summary>
public class GameInterface : BaseInterface
{
	[Header("Scene references")]
	public TextMeshProUGUI scoreDisplay;

	public void UpdateScore(float value)
	{
		scoreDisplay.text = Mathf.RoundToInt(value).ToString();
	}
}