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

	public void Init(bool initialSoundState, bool[] unlocks, int selectedIndex, Action startGame, Action reset, Action<int> giveMoney, Action<int> takeMoney, Action<bool> setSoundState, Action<Action> startFakeAd, Func<bool> canBuy)
	{
		gameInterface.Init();
		mainInterface.Init(
			initialSoundState,
			shopInterface.Show,
			() =>
			{
				startGame();

				mainInterface.Hide();
				gameInterface.Show();
			},
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
			() =>
			{
				reset();

				loseInterface.Hide();
				mainInterface.Show();
			},
			shopInterface.Show
		);

		InitInternal();
	}

	public void GameOver()
	{
		if(!CheckInitialized())
			return;

		gameInterface.Hide();
		loseInterface.Show();
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