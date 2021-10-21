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

	List<Transform> spawnedChunks;
	List<int> lastSpawnedChunks;
	Transform player;

	public void Init(Transform player)
	{
		this.player = player;

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

	Vector3 GetSpawnPos()
	{
		return new Vector3(0, 0, spawnPoint.position.z);
	}

	float GetDistanceFromPlayer(Transform chunk)
	{
		float zDifference = player.position.z - chunk.position.z;
		return zDifference > 0 ? zDifference : 0;
	}

	public void SpawnChunk(int difficulty)
	{
		if(!CheckInitialized())
			return;

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

		spawnedChunks.Add(spawnedChunk.transform);
	}
}