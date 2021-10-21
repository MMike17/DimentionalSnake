using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Manages the terrain generation, obstacles and bonuses</summary>
public class TerrainManager : BaseBehaviour
{
	// TODO : Move terrain chunks

	[Header("Settings")]
	public int memorySize;
	public float deleteDistance;

	[Header("Scene references")]
	public TerrainChunk[] terrainChunks;
	[Space]
	public Transform spawnPoint;

	List<TerrainChunk> spawnedChunks;
	List<int> lastSpawnedChunks;
	Func<float> GetDifficulty;
	Transform player;
	int playerCurrentObstacle;

	public void Init(Transform player, Func<float> getDifficulty)
	{
		this.player = player;
		GetDifficulty = getDifficulty;

		lastSpawnedChunks = new List<int>();
		spawnedChunks = new List<TerrainChunk>();
		playerCurrentObstacle = 0;

		InitInternal();
	}

	void Update()
	{
		if(!initialized)
			return;

		// clean chunk list
		List<TerrainChunk> toRemove = new List<TerrainChunk>();

		foreach (TerrainChunk chunk in spawnedChunks)
		{
			if(GetDistanceFromPlayer(chunk.transform) >= deleteDistance)
			{
				toRemove.Add(chunk);
				Destroy(chunk.gameObject);
			}
		}

		toRemove.ForEach(item => spawnedChunks.Remove(item));

		// detect necessary spawn
		bool shouldSpawn = false;

		spawnedChunks.ForEach(item =>
		{
			if(item.ShouldSpawnNew(player.position.z))
				shouldSpawn = true;
		});

		if(shouldSpawn)
			SpawnChunk();
	}

	Vector3 GetSpawnPos()
	{
		return new Vector3(0, 0, spawnPoint.position.z);
	}

	float GetDistanceFromPlayer(Transform chunk)
	{
		float zDifference = player.position.z - chunk.position.z;
		return zDifference > 0 ? zDifference : 0;
	}

	public void SpawnChunk()
	{
		if(!CheckInitialized())
			return;

		float difficulty = GetDifficulty();
		int chunkIndex = -1;

		// pick next index
		List<int> availableIndexes = new List<int>();

		for (int i = 0; i < terrainChunks.Length; i++)
			availableIndexes.Add(i);

		lastSpawnedChunks.ForEach(item => availableIndexes.Remove(item));

		chunkIndex = availableIndexes[UnityEngine.Random.Range(0, availableIndexes.Count)];
		lastSpawnedChunks.Add(chunkIndex);

		// keep memory size consistent
		if(lastSpawnedChunks.Count > memorySize)
			lastSpawnedChunks.RemoveAt(0);

		// spawn chunk
		TerrainChunk spawnedChunk = Instantiate(terrainChunks[chunkIndex], GetSpawnPos(), Quaternion.identity);
		spawnedChunk.Init(difficulty);

		spawnedChunks.Add(spawnedChunk);
	}
}