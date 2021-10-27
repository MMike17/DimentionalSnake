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
	Func<Vector3, float> GetPositionPercent;
	Func<Vector3, bool> IsBehindCamera;
	Action<Renderer> SetRendererToCamera;
	Action<float> AddDistance;
	float currentSpeed;

	public void Init(Action<float> addDistance, Action<Renderer> setRendererToCamera, Func<Vector3, bool> isBehindCamera, Func<Vector3, float> getPositionPercent)
	{
		AddDistance = addDistance;
		SetRendererToCamera = setRendererToCamera;
		IsBehindCamera = isBehindCamera;
		GetPositionPercent = getPositionPercent;

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
		TerrainChunk emptyChunk = Instantiate(emptyChunkPrefab, position, Quaternion.identity, transform);
		emptyChunk.Init();

		spawnedChunks.Add(emptyChunk);
	}

	Portal SpawnPortal(Vector3 position)
	{
		Portal portal = Instantiate(portalPrefab, position, Quaternion.identity);
		portal.Init(SetRendererToCamera, () => { return GetPositionPercent(portal.transform.position); }, IsBehindCamera);

		spawnedPortals.Add(portal.transform);
		return portal;
	}

	public void SpawnTerrain(float difficulty, float currentSpeed, Vector3 lastChunkPos, Snake player)
	{
		if(!CheckInitialized())
			return;

		this.currentSpeed = currentSpeed;

		// spawns start portal
		Transform[] playerPieces = player.GetPiecesTransforms();

		Portal startPortal = SpawnPortal(lastChunkPos);
		startPortal.name = "StartPortal";
		startPortal.StartAnimation(player.transform, playerPieces);

		// spawning empty chunk first
		float chunkSize = emptyChunkPrefab.transform.GetChild(0).localScale.z;
		Vector3 spawnPos = lastChunkPos + Vector3.forward * chunkSize;
		SpawnEmptyChunk(spawnPos);

		// spawning chunks
		float currentDifficulty = difficulty + Mathf.Lerp(difficulty, 1, 0.5f);
		int currentSize = Mathf.RoundToInt(Mathf.Lerp(minTerrainSize, maxTerrainSize, difficulty));

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

			TerrainChunk spawnedChunk = Instantiate(terrainChunks[index], spawnPos, Quaternion.identity, transform);
			spawnedChunk.Init(currentDifficulty);

			spawnedChunks.Add(spawnedChunk);
		}

		// spawns end portal
		Portal endPortal = SpawnPortal(spawnPos);
		endPortal.name = "EndPortal";

		// spawns pieces copies
		Transform[] copiedPieces = new Transform[10];

		for (int i = 0; i < 10; i++)
		{
			Transform pieceCopy = Instantiate(playerPieces[i]);

			// position new pieces
			pieceCopy.SetParent(endPortal.transform);
			pieceCopy.position = endPortal.targetPositions[i].position;

			copiedPieces[i] = pieceCopy;
		}

		// link pieces
		for (int i = 0; i < 9; i++)
			copiedPieces[i].GetComponent<SnakePiece>().Init(copiedPieces[i + 1], 0);

		copiedPieces[copiedPieces.Length - 1].GetComponent<SnakePiece>().Init(copiedPieces[0], 0);
	}

	public void DestroyTerrain()
	{
		if(!CheckInitialized())
			return;

		spawnedChunks.ForEach(item => Destroy(item.gameObject));
		spawnedChunks.Clear();
	}
}