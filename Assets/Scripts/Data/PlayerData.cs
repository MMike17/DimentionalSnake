using System;

/// <summary>Class used to save player data</summary>
[Serializable]
public class PlayerData
{
	public float highscore;
	public int playerMoney, selectedSettings;
	public bool hasSound;
	public bool[] unlockedSettings;

	public PlayerData(int settingsCount)
	{
		highscore = 0;
		playerMoney = 0;
		selectedSettings = 0;
		hasSound = true;

		unlockedSettings = new bool[settingsCount];
		unlockedSettings[0] = true;
	}
}