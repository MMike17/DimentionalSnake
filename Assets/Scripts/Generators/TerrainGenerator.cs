using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Generates terrain</summary>
public class TerrainGenerator : BaseBehaviour
{
	[Header("Settings")]
	public int memorySize;

	[Header("Scene references")]
	public TerrainChunk[] terrainChunks;
	public Transform spawnPoint;

	Vector3 spawnPos => new Vector3(0, 0, spawnPoint.position.z);

	List<Transform> spawnedChunks;
	List<int> lastSpawnedChunks;
	Func<Transform, float> GetDistanceFromPlayer;
	float deleteDistance;

	public void Init(float deleteDistance, Func<Transform, float> getDistanceFromPlayer)
	{
		this.deleteDistance = deleteDistance;
		GetDistanceFromPlayer = getDistanceFromPlayer;

		lastSpawnedChunks = new List<int>();
		spawnedChunks = new List<Transform>();

		InitInternal();
	}

	void Update()
	{
		if(!initialized)
			return;

		// clean chunk list
		List<Transform> toRemove = new List<Transform>();

		foreach (Transform chunk in spawnedChunks)
		{
			if(GetDistanceFromPlayer(chunk) >= deleteDistance)
			{
				toRemove.Add(chunk);
				Destroy(chunk.gameObject);
			}
		}

		toRemove.ForEach(item => spawnedChunks.Remove(item));
	}

	public void SpawnChunk(int difficulty)
	{
		if(!CheckInitialized())
			return;

		int chunkIndex = -1;

		// pick next index
		List<int> availableIndexes = new List<int>();

		for (int i = 0; i < terrainChunks.Length; i++)
		{
			if(terrainChunks[i].difficultyRating == difficulty)
				availableIndexes.Add(i);
		}

		lastSpawnedChunks.ForEach(item => availableIndexes.Remove(item));

		chunkIndex = availableIndexes[UnityEngine.Random.Range(0, availableIndexes.Count)];
		lastSpawnedChunks.Add(chunkIndex);

		// keep memory size consistent
		if(lastSpawnedChunks.Count > memorySize)
			lastSpawnedChunks.RemoveAt(0);

		// spawn terrain chunk
		Transform spawnedChunk = Instantiate(terrainChunks[chunkIndex].terrainPrefab, spawnPos, Quaternion.identity).transform;

		spawnedChunks.Add(spawnedChunk);
	}

	/// <summary>Represents a terrain chunk</summary>
	[Serializable]
	public class TerrainChunk
	{
		public int difficultyRating;
		public GameObject terrainPrefab;
	}
}