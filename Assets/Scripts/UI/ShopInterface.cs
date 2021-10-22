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

	bool[] unlocks;

	public void Init(bool[] unlocks, int selectedIndex)
	{
		this.unlocks = unlocks;

		UpdateUnlocks();
		SpawnTickets();

		InitInternal();
	}

	void UpdateUnlocks()
	{
		int unlockedCount = 0;

		foreach (bool unlock in unlocks)
			unlockedCount++;

		unlockSlider.value = (float) unlockedCount / snakeSettings.Length;
		unlockCount.text = string.Format(COUNT_FORMAT, unlockedCount, snakeSettings.Length);
	}

	void SpawnTickets()
	{
		// TODO : Spawn snake settings tickets here
	}
}