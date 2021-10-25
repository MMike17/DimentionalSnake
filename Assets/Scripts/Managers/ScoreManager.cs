using System;
using UnityEngine;

/// <summary>Manages the player score and highscore</summary>
public class ScoreManager : BaseBehaviour
{
	[Header("Settings")]
	public float initialHighscore;

	Action TriggerHighscore;
	Action<float> UpdateScore;
	Action<int> UpdateMoney;
	float currentPlayerScore, playerHighscore, distanceToSpawn;
	int currentPlayerMoney;
	bool passedHighscore, useDefaultHighscore;

	public void Init(float lastHighscore, float distanceToSpawn, int lastMoney, Action triggerHighscore, Action<float> updateScore, Action<int> updateMoney)
	{
		playerHighscore = lastHighscore;
		useDefaultHighscore = lastHighscore == 0;
		this.distanceToSpawn = distanceToSpawn;
		currentPlayerMoney = lastMoney;
		TriggerHighscore = triggerHighscore;
		UpdateScore = updateScore;
		UpdateMoney = updateMoney;

		passedHighscore = false;

		UpdateScore(lastHighscore);
		UpdateMoney(lastMoney);

		InitInternal();
	}

	public float GetCurrentScore()
	{
		if(!CheckInitialized())
			return 0;

		return currentPlayerScore;
	}

	public void GiveMoney(int amount = 1)
	{
		if(!CheckInitialized())
			return;

		currentPlayerMoney += amount;

		UpdateMoney(currentPlayerMoney);
	}

	public int GetMoney()
	{
		if(!CheckInitialized())
			return 0;

		return currentPlayerMoney;
	}

	public void TakeMoney(int amount)
	{
		if(!CheckInitialized())
			return;

		currentPlayerMoney -= amount;

		UpdateMoney(currentPlayerMoney);
	}

	public bool CanBuy(int price)
	{
		return currentPlayerMoney >= price;
	}

	public void AddPlayerScore(float score)
	{
		if(!CheckInitialized())
			return;

		currentPlayerScore += score / 100;

		UpdateScore(currentPlayerScore);

		if(currentPlayerScore >= playerHighscore)
			playerHighscore = currentPlayerScore;

		if(currentPlayerScore + distanceToSpawn / 10 >= (useDefaultHighscore ? initialHighscore : playerHighscore))
		{
			if(!passedHighscore)
			{
				useDefaultHighscore = false;
				passedHighscore = true;
				TriggerHighscore();
			}
		}
	}

	public float GetHighscore()
	{
		if(!CheckInitialized())
			return 0;

		return useDefaultHighscore ? 0 : playerHighscore;
	}
}