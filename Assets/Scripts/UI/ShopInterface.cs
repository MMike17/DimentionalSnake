using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Manages what is displayed on the interface during the shop menu</summary>
public class ShopInterface : BaseInterface
{
	const string COUNT_FORMAT = "{0} / {1}";

	[Header("Settings")]
	public SnakePieces[] snakeSettings;

	[Header("Scene references")]
	public Slider unlockSlider;
	public TextMeshProUGUI unlockCount;
	public VerticalLayoutGroup listHolder;
	public Transform linePrefab;
	public SettingsTicket ticketPrefab;
	public Button watchAdButton, returnButton;
	[Space]
	public Button rewardPanel;

	List<SettingsTicket> spawnedTickets;
	bool[] unlocks;
	int playerSelectedIndex;

	public void Init(bool[] unlocks, int selectedIndex, Action<Action> startFakeAd, Action<int> giveMoney, Action<int> takeMoney, Action<int> selectIndex, Func<bool> canBuy)
	{
		this.unlocks = unlocks;
		this.playerSelectedIndex = selectedIndex;

		UpdateUnlocks();
		SpawnTickets(selectedIndex, takeMoney, selectIndex, canBuy);

		watchAdButton.onClick.AddListener(() => startFakeAd(() => rewardPanel.gameObject.SetActive(true)));
		returnButton.onClick.AddListener(() => Hide());

		rewardPanel.onClick.AddListener(() =>
		{
			rewardPanel.gameObject.SetActive(false);
			giveMoney(20);
		});

		InitInternal();
	}

	void Update()
	{
		foreach (LayoutGroup layout in FindObjectsOfType<LayoutGroup>())
			LayoutRebuilder.ForceRebuildLayoutImmediate(layout.GetComponent<RectTransform>());
	}

	void UpdateUnlocks()
	{
		int unlockedCount = 0;

		foreach (bool unlock in unlocks)
		{
			if(unlock)
				unlockedCount++;
		}

		unlockSlider.value = (float) unlockedCount / snakeSettings.Length;
		unlockCount.text = string.Format(COUNT_FORMAT, unlockedCount, snakeSettings.Length);
	}

	void SpawnTickets(int selectedIndex, Action<int> takeMoney, Action<int> selectIndex, Func<bool> canBuy)
	{
		// spawn lines
		int linesCount = Mathf.CeilToInt(snakeSettings.Length / 3);

		for (int i = 0; i < linesCount; i++)
			Instantiate(linePrefab, listHolder.transform);

		// spawn tickets
		int currentLine = 0;

		float totalWidth = listHolder.GetComponent<RectTransform>().rect.width;
		float ticketWidth = (totalWidth - totalWidth / 10);

		listHolder.spacing = (totalWidth - ticketWidth) / 2;
		ticketWidth /= 3;

		spawnedTickets = new List<SettingsTicket>();

		for (int i = 0; i < snakeSettings.Length; i++)
		{
			// go to next line
			if(i != 0 && i % 3 == 0)
				currentLine++;

			int index = i;

			SettingsTicket ticket = Instantiate(ticketPrefab, listHolder.transform.GetChild(currentLine));
			ticket.Init(
				snakeSettings[index],
				unlocks[index],
				index == selectedIndex,
				ticketWidth,
				() =>
				{
					unlocks[index] = true;
					playerSelectedIndex = index;

					spawnedTickets.ForEach(item => item.Unselect());

					takeMoney(200);
				},
				() =>
				{
					spawnedTickets.ForEach(item => item.Unselect());

					playerSelectedIndex = index;
					selectIndex(index);
				},
				canBuy
			);

			spawnedTickets.Add(ticket);
		}
	}

	public ShopPlayerSetting GetShopPlayerSetting()
	{
		return new ShopPlayerSetting(unlocks, playerSelectedIndex);
	}

	public struct ShopPlayerSetting
	{
		public bool[] unlocks;
		public int selectedIndex;

		public ShopPlayerSetting(bool[] unlocks, int selectedIndex)
		{
			this.unlocks = unlocks;
			this.selectedIndex = selectedIndex;
		}
	}
}