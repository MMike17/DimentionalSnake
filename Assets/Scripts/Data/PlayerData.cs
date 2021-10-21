using System;

/// <summary>Class used to save player data</summary>
[Serializable]
public class PlayerData
{
	public float highscore;
	public int playerMoney;

	public PlayerData()
	{
		highscore = 0;
		playerMoney = 0;
	}
}