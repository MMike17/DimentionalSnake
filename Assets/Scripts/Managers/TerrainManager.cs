using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Manages the terrain generation, obstacles and bonuses</summary>
public class TerrainManager : BaseBehaviour
{
	[Header("Settings")]
	public int memorySize;
	public float deleteDistance;

	[Header("Scene references")]
	public Chunk[] obstaclesChunks;
	[Space]
	public Transform spawnPoint; // The spawn point MUST be child of the player (constant distance from player)

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
		Vector3 playerPos = player.position;
		playerPos.x = 0;
		playerPos.y = 0;

		return Vector3.Distance(playerPos, chunk.position);
	}

	public void SpawnChunk(int difficulty)
	{
		if(!CheckInitialized())
			return;

		int chunkIndex = -1;

		// pick next index
		List<int> availableIndexes = new List<int>();

		for (int i = 0; i < obstaclesChunks.Length; i++)
		{
			if(obstaclesChunks[i].difficultyRating == difficulty)
				availableIndexes.Add(i);
		}

		lastSpawnedChunks.ForEach(item => availableIndexes.Remove(item));

		chunkIndex = availableIndexes[UnityEngine.Random.Range(0, availableIndexes.Count)];
		lastSpawnedChunks.Add(chunkIndex);

		// keep memory size consistent
		if(lastSpawnedChunks.Count > memorySize)
			lastSpawnedChunks.RemoveAt(0);

		// spawn chunk
		Transform spawnedChunk = Instantiate(obstaclesChunks[chunkIndex].Prefab, GetSpawnPos(), Quaternion.identity).transform;

		spawnedChunks.Add(spawnedChunk);
	}

	/// <summary>Represents a chunk</summary>
	[Serializable]
	public class Chunk
	{
		public int difficultyRating;
		public GameObject Prefab;
	}
}