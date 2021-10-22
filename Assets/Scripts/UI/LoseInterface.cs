using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Manages what is displayed on the interface when the player loses</summary>
public class LoseInterface : BaseInterface
{
	const string STORE_URL = "https://apps.apple.com/us/app/space-snake/id1317018868";
	const string HIGHSCORE_FORMAT = "Highscore : {0}";

	[Header("Scene references")]
	public TextMeshProUGUI scoreDisplay;
	public TextMeshProUGUI highscoreDisplay;
	public Button replayButton, shopButton, storeButton;

	public void Init(Action replay, Action openShop)
	{
		replayButton.onClick.AddListener(() => replay());
		shopButton.onClick.AddListener(() => openShop());
		storeButton.onClick.AddListener(() => Application.OpenURL(STORE_URL));

		InitInternal();
	}

	public void SetData(float score, float highscore)
	{
		scoreDisplay.text = score.ToString();
		highscoreDisplay.text = string.Format(HIGHSCORE_FORMAT, highscore);
	}
}