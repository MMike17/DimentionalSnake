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

	public void Init(int difficulty)
	{
		// decide which obstacles need to be spawned
		List<int> selectedIndexes = GetSelectedIndexes(obstacles.Length, obstaclesPerDifficulty[difficulty], difficulty);

		for (int i = 0; i < obstacles.Length; i++)
			obstacles[i].SetActive(selectedIndexes.Contains(i));

		// decide which pieces need to be spawned
		selectedIndexes = GetSelectedIndexes(pieces.Length, piecesPerDifficulty[difficulty], difficulty);

		for (int i = 0; i < pieces.Length; i++)
			pieces[i].SetActive(selectedIndexes.Contains(i));

		InitInternal();
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
}