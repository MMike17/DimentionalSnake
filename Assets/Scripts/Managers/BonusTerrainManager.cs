using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Manages bonus level spawning and animations</summary>
public class BonusTerrainManager : BaseBehaviour
{
	[Header("Settings")]
	public int minTerrainSize;
	public int maxTerrainSize;

	[Header("Scene references")]
	public TerrainChunk emptyChunkPrefab;
	public TerrainChunk[] terrainChunks;
	public Portal portalPrefab;

	List<TerrainChunk> spawnedChunks;
	List<Transform> spawnedPortals;
	Func<Vector3, bool> IsBehindCamera;
	Action<Renderer> SetRendererToCamera;
	Action<float> AddDistance;
	float currentSpeed;

	void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;

		Gizmos.DrawLine(transform.position + Vector3.up * 0.1f, transform.position + Vector3.up * 0.1f + transform.forward * minTerrainSize);
		Gizmos.DrawLine(transform.position, transform.position + transform.forward * maxTerrainSize);
	}

	public void Init(Action<float> addDistance, Action<Renderer> setRendererToCamera, Func<Vector3, bool> isBehindCamera)
	{
		AddDistance = addDistance;
		SetRendererToCamera = setRendererToCamera;
		IsBehindCamera = isBehindCamera;

		spawnedChunks = new List<TerrainChunk>();
		spawnedPortals = new List<Transform>();

		InitInternal();
	}

	void Update()
	{
		if(!initialized)
			return;

		// move portals back
		List<Transform> toRemove = new List<Transform>();

		spawnedPortals.ForEach(item =>
		{
			if(item == null)
				toRemove.Add(item);
			else
				item.Translate(0, 0, -currentSpeed * Time.deltaTime);
		});

		// clean portal list
		toRemove.ForEach(item => spawnedPortals.Remove(item));
	}

	void SpawnEmptyChunk(Vector3 position)
	{
		TerrainChunk emptyChunk = Instantiate(emptyChunkPrefab, transform.position, Quaternion.identity);
		emptyChunk.Init();

		spawnedChunks.Add(emptyChunk);
	}

	void SpawnPortal(Vector3 position, Transform player, Transform[] playerPieces)
	{
		Portal startPortal = Instantiate(portalPrefab, position, Quaternion.identity);
		startPortal.Init(SetRendererToCamera, IsBehindCamera);
		startPortal.StartAnimation(player, playerPieces);

		spawnedPortals.Add(startPortal.transform);
	}

	public void SpawnTerrain(float difficulty, float currentSpeed, Vector3 lastChunkPos, Snake player)
	{
		if(!CheckInitialized())
			return;

		this.currentSpeed = currentSpeed;

		// spawns start portal
		Transform[] playerPieces = player.GetPiecesTransforms();
		SpawnPortal(lastChunkPos, player.transform, playerPieces);

		// spawning empty chunk first
		float chunkSize = emptyChunkPrefab.transform.GetChild(0).localScale.z;
		SpawnEmptyChunk(lastChunkPos + Vector3.forward * chunkSize);

		// spawning chunks
		float currentDifficulty = difficulty + Mathf.Lerp(difficulty, 1, 0.5f);
		int currentSize = Mathf.RoundToInt(Mathf.Lerp(minTerrainSize, maxTerrainSize, difficulty));

		Vector3 spawnPos = spawnedChunks[0].transform.position;
		List<int> availableIndexes = new List<int>();

		for (int i = 0; i < terrainChunks.Length; i++)
			availableIndexes.Add(i);

		for (int i = 1; i < currentSize; i++)
		{
			// pick index
			int index = availableIndexes[UnityEngine.Random.Range(0, availableIndexes.Count)];
			availableIndexes.Remove(index);

			// regenerate indexes
			if(availableIndexes.Count == 0)
			{
				for (int j = 0; j < terrainChunks.Length; j++)
					availableIndexes.Add(j);
			}

			// spawn chunk
			spawnPos += Vector3.forward * chunkSize;

			TerrainChunk spawnedChunk = Instantiate(terrainChunks[index], spawnPos, Quaternion.identity);
			spawnedChunk.Init(currentDifficulty);

			spawnedChunks.Add(spawnedChunk);
		}

		// spawns end portal
		SpawnPortal(spawnPos, player.transform, playerPieces);
	}

	public void DestroyTerrain()
	{
		if(!CheckInitialized())
			return;

		spawnedChunks.ForEach(item => Destroy(item.gameObject));
		spawnedChunks.Clear();
	}
}