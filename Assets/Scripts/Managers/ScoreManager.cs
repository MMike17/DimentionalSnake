using UnityEngine;

/// <summary>Manages the player score and highscore</summary>
public class ScoreManager : BaseBehaviour
{
	// TODO : trigger highscore feedback

	float currentPlayerScore;
	int currentPlayerMoney;

	public void Init()
	{
		InitInternal();
	}

	public float GetCurrentScore()
	{
		if(!CheckInitialized())
			return 0;

		return currentPlayerScore;
	}

	public void GetMoney()
	{
		if(!CheckInitialized())
			return;

		currentPlayerMoney++;
	}

	public void AddPlayerScore(float score)
	{
		if(!CheckInitialized())
			return;

		currentPlayerScore += score;
	}
}