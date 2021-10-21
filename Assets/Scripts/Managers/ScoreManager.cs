using System;
using UnityEngine;

/// <summary>Manages the player score and highscore</summary>
public class ScoreManager : BaseBehaviour
{
	[Header("Settings")]
	public float initialHighscore;

	Action TriggerHighscore;
	float currentPlayerScore, playerHighscore;
	int currentPlayerMoney;
	bool passedHighscore;

	public void Init(float lastHighscore, Action triggerHighscore)
	{
		playerHighscore = lastHighscore;
		TriggerHighscore = triggerHighscore;

		passedHighscore = false;

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

		if(currentPlayerScore >= playerHighscore)
		{
			if(passedHighscore)
				playerHighscore = currentPlayerScore;
			else
			{
				passedHighscore = true;
				TriggerHighscore();
			}
		}
	}

	public float GetHighscore()
	{
		if(!CheckInitialized())
			return 0;

		return playerHighscore;
	}
}