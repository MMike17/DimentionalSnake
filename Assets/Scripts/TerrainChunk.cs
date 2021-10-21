using System.Collections.Generic;
using UnityEngine;

/// <summary>Manages the difficulty and modulation of a terrain chunk</summary>
public class TerrainChunk : BaseBehaviour
{
	[Header("Settings")]
	public int[] obstaclesPerDifficulty;
	public int[] piecesPerDifficulty;

	[Header("Scene references")]
	public GameObject[] obstacles;
	public GameObject[] pieces;

	bool triggeredSpawnNew;

	public void Init(float difficulty)
	{
		triggeredSpawnNew = false;
		List<int> selectedIndexes;

		// decide which obstacles need to be spawned
		if(obstacles.Length != 0)
		{
			int obstaclesDifficulty = GetObstaclesDifficulty(difficulty);
			selectedIndexes = GetSelectedIndexes(obstacles.Length, obstaclesPerDifficulty[obstaclesDifficulty], obstaclesDifficulty);

			for (int i = 0; i < obstacles.Length; i++)
				obstacles[i].SetActive(selectedIndexes.Contains(i));
		}

		// decide which pieces need to be spawned
		if(pieces.Length != 0)
		{
			int piecesDifficulty = GetPiecesDifficulty(difficulty);
			selectedIndexes = GetSelectedIndexes(pieces.Length, piecesPerDifficulty[piecesDifficulty], piecesDifficulty);

			for (int i = 0; i < pieces.Length; i++)
				pieces[i].SetActive(selectedIndexes.Contains(i));
		}

		InitInternal();
	}

	int GetObstaclesDifficulty(float difficulty)
	{
		return Mathf.FloorToInt(difficulty * obstaclesPerDifficulty.Length);
	}

	int GetPiecesDifficulty(float difficulty)
	{
		return Mathf.FloorToInt(difficulty * piecesPerDifficulty.Length);
	}

	List<int> GetSelectedIndexes(int arrayCount, int pickCount, int difficulty)
	{
		List<int> selectedIndexes = new List<int>();
		List<int> availableIndexes = new List<int>();

		for (int i = 0; i < arrayCount; i++)
			availableIndexes.Add(i);

		for (int i = 0; i < pickCount; i++)
		{
			int selectedIndex = availableIndexes[Random.Range(0, availableIndexes.Count)];

			selectedIndexes.Add(selectedIndex);
			availableIndexes.Remove(selectedIndex);
		}

		return selectedIndexes;
	}

	public bool ShouldSpawnNew(float playerZPos)
	{
		if(triggeredSpawnNew)
			return false;
		else if(transform.position.z <= playerZPos)
		{
			triggeredSpawnNew = true;
			return true;
		}
		else
			return false;
	}
}