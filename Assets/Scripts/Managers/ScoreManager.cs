using UnityEngine;

/// <summary>Manages the player score and highscore</summary>
public class ScoreManager : BaseBehaviour
{
	// TODO : store player score
	// TODO : trigger highscore feedback

	public void Init()
	{
		InitInternal();
	}

	public float GetCurrentScore()
	{
		if(!CheckInitialized())
			return 0;

		// TODO : return current score

		return 0;
	}
}