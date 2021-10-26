using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Manages bonus level spawning and animations</summary>
public class BonusTerrainManager : BaseBehaviour
{
	// TODO : Manage render textures
	// TODO : Rework this

	[Header("Settings")]
	public int minTerrainSize;
	public int maxTerrainSize;

	[Header("Scene references")]
	public TerrainChunk emptyChunkPrefab;
	public TerrainChunk[] bonusChunks;
	public Transform spawnPoint, playerTarget;

	List<TerrainChunk> spawnedChunks;
	Action<float> AddDistance;

	void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		SetGizmosAlpha(0.5f);

		if(spawnPoint != null)
			Gizmos.DrawSphere(spawnPoint.position, 0.3f);

		if(playerTarget != null)
			Gizmos.DrawSphere(playerTarget.position, 0.2f);
	}

	public void Init(Action<float> addDistance)
	{
		AddDistance = addDistance;

		spawnedChunks = new List<TerrainChunk>();

		InitInternal();
	}

	public void SpawnTerrain(float difficulty)
	{
		if(!CheckInitialized())
			return;

		float currentDifficulty = difficulty + Mathf.Lerp(difficulty, 1, 0.5f);
		int currentSize = Mathf.RoundToInt(Mathf.Lerp(minTerrainSize, maxTerrainSize, difficulty));

		// spawning empty chunk first
		TerrainChunk emptyChunk = Instantiate(emptyChunkPrefab, transform.position, Quaternion.identity);
		emptyChunk.Init();

		spawnedChunks.Add(emptyChunk);

		// spawning chunks
		List<int> availableIndexes = new List<int>();

		for (int i = 0; i < bonusChunks.Length; i++)
			availableIndexes.Add(i);

		float chunkSize = emptyChunkPrefab.transform.GetChild(0).localScale.z;
		Vector3 spawnPos = spawnPoint.position;

		for (int i = 1; i < currentSize; i++)
		{
			// pick index
			int index = availableIndexes[UnityEngine.Random.Range(0, availableIndexes.Count)];
			availableIndexes.Remove(index);

			// regenerate indexes
			if(availableIndexes.Count == 0)
			{
				for (int j = 0; j < bonusChunks.Length; j++)
					availableIndexes.Add(j);
			}

			// spawn chunk
			TerrainChunk spawnedChunk = Instantiate(bonusChunks[index], spawnPos, spawnPoint.rotation, spawnPoint);
			spawnedChunk.Init(currentDifficulty);

			spawnedChunks.Add(spawnedChunk);

			spawnPos += Vector3.forward * chunkSize;
		}

		// TODO : Add portal at end of path
	}

	public void DestroyTerrain()
	{
		if(!CheckInitialized())
			return;

		// TODO : Destroy bonus terrain after bonus
	}
}