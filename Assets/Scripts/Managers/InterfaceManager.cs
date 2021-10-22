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
	public ShopInterface shopInterface;

	public void Init(bool initialSoundState, Action startGame, Action<bool> setSoundState)
	{
		gameInterface.Init();
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

	public int GetUnlocksCount()
	{
		if(!CheckInitialized())
			return 0;

		return shopInterface.snakeSettings.Length;
	}
}