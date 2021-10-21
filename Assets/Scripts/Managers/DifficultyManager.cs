using System;
using UnityEngine;

/// <summary></summary>
public class DifficultyManager : BaseBehaviour
{
	[Header("Settings")]
	public float minSpeed;
	public float maxSpeed;

	Func<float> GetCurrentDistance;
	float highscore, currentDifficulty;

	public void Init(float highscore, Func<float> getCurrentDistance)
	{
		this.highscore = highscore;
		GetCurrentDistance = getCurrentDistance;

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

		return Mathf.Lerp(minSpeed, maxSpeed, currentDifficulty);
	}
}