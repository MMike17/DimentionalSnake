using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Manages the terrain generation, obstacles and bonuses</summary>
public class TerrainManager : BaseBehaviour
{
	// TODO : Spawn empty at first and then obstacles

	[Header("Settings")]
	public int memorySize;
	public float deleteDistance;

	[Header("Scene references")]
	public TerrainChunk emptyChunkPrefab;
	public TerrainChunk[] terrainChunks;
	public Transform minX, maxX;
	public GameObject newHighscorePrefab;
	[Space]
	public Transform spawnPoint;

	List<TerrainChunk> spawnedChunks;
	List<int> lastSpawnedChunks;
	Func<float> GetDifficulty, GetCurrentSpeed;
	Action<float> AddDistance;
	Transform player, newHighscoreTransform;
	bool canMove;

	void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		SetGizmosAlpha(0.5f);

		if(spawnPoint != null)
			Gizmos.DrawSphere(spawnPoint.position, 1f);

		if(minX != null)
			Gizmos.DrawSphere(minX.position, 0.5f);

		if(maxX != null)
			Gizmos.DrawSphere(maxX.position, 0.5f);

		if(minX != null && maxX != null)
			Gizmos.DrawLine(minX.position, maxX.position);
	}

	public void Init(Transform player, Func<float> getDifficulty, Func<float> getCurrentSpeed, Action<float> addDistance)
	{
		this.player = player;
		GetDifficulty = getDifficulty;
		GetCurrentSpeed = getCurrentSpeed;
		AddDistance = addDistance;

		lastSpawnedChunks = new List<int>();
		spawnedChunks = new List<TerrainChunk>();
		canMove = false;

		InitInternal();

		Reset();
	}

	void Update()
	{
		if(!initialized || !canMove)
			return;

		List<TerrainChunk> toRemove = new List<TerrainChunk>();

		foreach (TerrainChunk chunk in spawnedChunks)
		{
			// move chunks
			chunk.transform.Translate(0, 0, -GetCurrentSpeed() * Time.deltaTime);
			AddDistance(GetCurrentSpeed() * Time.deltaTime);

			// should remove chunk
			if(GetDistanceFromPlayer(chunk.transform) >= deleteDistance)
			{
				toRemove.Add(chunk);
				Destroy(chunk.gameObject);
			}
		}

		// clean chunk list
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

		// move highscore
		if(newHighscoreTransform != null)
		{
			newHighscoreTransform.Translate(0, 0, -GetCurrentSpeed() * Time.deltaTime);

			// destroy highscore
			if(GetDistanceFromPlayer(newHighscoreTransform) >= deleteDistance)
				Destroy(newHighscoreTransform.gameObject);
		}
	}

	Vector3 GetSpawnPos()
	{
		Vector3 position = spawnPoint.position;
		Transform lastSpawnedChunk = spawnedChunks[spawnedChunks.Count - 1].transform;
		position.z = lastSpawnedChunk.position.z + lastSpawnedChunk.GetChild(0).localScale.z;

		return position;
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
		TerrainChunk spawnedChunk = Instantiate(terrainChunks[chunkIndex], GetSpawnPos(), Quaternion.identity, transform);
		spawnedChunk.Init(difficulty);

		spawnedChunks.Add(spawnedChunk);
	}

	public void SpawnNewHighscore()
	{
		if(!CheckInitialized())
			return;

		newHighscoreTransform = Instantiate(newHighscorePrefab, spawnPoint.position, spawnPoint.rotation).transform;
	}

	public void Reset()
	{
		if(!CheckInitialized())
			return;

		// destroy previous chunks
		spawnedChunks.ForEach(item => Destroy(item.gameObject));
		spawnedChunks.Clear();

		// spawn new chunks
		float emptyChunkSize = emptyChunkPrefab.transform.GetChild(0).localScale.z;
		int spawnCount = Mathf.RoundToInt((spawnPoint.position.z - player.position.z) / emptyChunkSize);
		Vector3 position = spawnPoint.position;

		for (int i = 0; i < spawnCount; i++)
		{
			position -= Vector3.forward * emptyChunkSize;
			TerrainChunk spawnedChunk = Instantiate(emptyChunkPrefab, position, Quaternion.identity, transform);

			spawnedChunks.Insert(0, spawnedChunk);
		}
	}

	public void Freeze()
	{
		if(!CheckInitialized())
			return;

		canMove = false;
	}

	public void Unfreeze()
	{
		if(!CheckInitialized())
			return;

		Reset();
		canMove = true;
	}
}