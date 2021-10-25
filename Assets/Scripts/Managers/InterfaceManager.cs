using System;
using TMPro;
using UnityEngine;

/// <summary>Manages all display on interface</summary>
public class InterfaceManager : BaseBehaviour
{
	const float INTRO_ANIM_DURATION = 7.5f;

	[Header("Scene references")]
	public TextMeshProUGUI moneyDisplay;
	[Space]
	public GameInterface gameInterface;
	public MainInterface mainInterface;
	public ShopInterface shopInterface;
	public LoseInterface loseInterface;
	public GameObject introInterface;

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

		gameInterface.Hide();
		mainInterface.Hide();
		shopInterface.Hide();
		loseInterface.Hide();
		introInterface.SetActive(true);
		moneyDisplay.transform.parent.gameObject.SetActive(false);

		DelayedActionsManager.SceduleAction(() =>
		{
			introInterface.SetActive(false);
			moneyDisplay.transform.parent.gameObject.SetActive(true);

			mainInterface.Show();
		}, INTRO_ANIM_DURATION);
	}

	public void GameOver(float score, float highscore)
	{
		if(!CheckInitialized())
			return;

		gameInterface.Hide();
		loseInterface.Show();
		loseInterface.SetData(score, highscore);
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
		return shopInterface.snakeSettings.Length;
	}
}