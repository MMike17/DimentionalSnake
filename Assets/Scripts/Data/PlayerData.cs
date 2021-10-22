using System;

/// <summary>Class used to save player data</summary>
[Serializable]
public class PlayerData
{
	public float highscore;
	public int playerMoney;
	public bool hasSound;

	public PlayerData()
	{
		highscore = 0;
		playerMoney = 0;
		hasSound = true;
	}
}