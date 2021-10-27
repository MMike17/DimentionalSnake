using System;
using UnityEngine;

/// <summary>Manages the difficulty of the game depending on player score</summary>
public class DifficultyManager : BaseBehaviour
{
	[Header("Settings")]
	public float minSpeed;
	public float maxSpeed;

	Func<float> GetCurrentDistance;
	float highscore, currentDifficulty;
	bool forceDifficulty;

	public void Init(float highscore, Func<float> getCurrentDistance)
	{
		this.highscore = highscore;
		GetCurrentDistance = getCurrentDistance;

		forceDifficulty = false;

		InitInternal();
	}

	void Update()
	{
		if(!initialized)
			return;

		float currentDistance = GetCurrentDistance();

		currentDifficulty = currentDistance >= highscore ? 1 : currentDistance / highscore;
	}

	public void SetHighscore(float highscore)
	{
		if(!CheckInitialized())
			return;

		this.highscore = highscore;
	}

	public float GetDifficulty()
	{
		if(!CheckInitialized())
			return 0;

		return currentDifficulty;
	}

	public float GetCurrentSpeed()
	{
		if(!CheckInitialized())
			return 0;

		return forceDifficulty ? minSpeed / 2 : Mathf.Lerp(minSpeed, maxSpeed, currentDifficulty);
	}

	public float GetSpeedFromDifficulty(float difficulty)
	{
		if(!CheckInitialized())
			return 0;

		return Mathf.Lerp(minSpeed, maxSpeed, difficulty);
	}

	public void ForceSpeedDown()
	{
		if(!CheckInitialized())
			return;

		forceDifficulty = true;
	}

	public void Reset()
	{
		if(!CheckInitialized())
			return;

		forceDifficulty = false;
	}
}