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
	public LoseInterface loseInterface;

	public void Init(bool initialSoundState, bool[] unlocks, int selectedIndex, Action startGame, Action<int> giveMoney, Action<int> takeMoney, Action<bool> setSoundState, Action<Action> startFakeAd, Func<bool> canBuy)
	{
		gameInterface.Init();
		mainInterface.Init(
			initialSoundState,
			shopInterface.Show,
			startGame,
			setSoundState
		);
		shopInterface.Init(
			unlocks,
			selectedIndex,
			startFakeAd,
			giveMoney,
			takeMoney,
			canBuy
		);
		loseInterface.Init(
			startGame,
			shopInterface.Show
		);

		InitInternal();
	}

	public void StartGame()
	{
		mainInterface.Hide();
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