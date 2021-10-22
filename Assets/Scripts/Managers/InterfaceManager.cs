using System;
using TMPro;
using UnityEngine;

/// <summary>Manages all display on interface</summary>
public class InterfaceManager : BaseBehaviour
{
	[Header("Scene references")]
	public TextMeshProUGUI moneyDisplay;
	[Space]
	public GameInterface gameInterface;
	public MainInterface mainInterface;

	public void Init(bool initialSoundState, Action startGame, Action<bool> setSoundState)
	{
		mainInterface.Init(
			initialSoundState,
			() =>
			{
				// TODO : open shop here
			},
			startGame,
			setSoundState
		);

		InitInternal();
	}

	public void StartGame()
	{
		gameInterface.Show();
	}

	public void UpdateScore(float value)
	{
		if(!CheckInitialized())
			return;

		gameInterface.scoreDisplay.text = Mathf.RoundToInt(value).ToString();
	}

	public void UpdateMoney(int value)
	{
		if(!CheckInitialized())
			return;

		moneyDisplay.text = value.ToString();
	}
}