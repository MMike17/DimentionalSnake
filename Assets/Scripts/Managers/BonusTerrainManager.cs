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
	Portal startPortal, endPortal;
	Func<Vector3, float> GetPositionPercent;
	Func<Vector3, bool> IsBehindCamera;
	Func<float> GetCurrentSpeed;
	Action<Renderer, bool> SetRendererToCamera;
	Action<float> AddDistance;
	float currentSpeed;
	int bonusLayer, normalLayer;
	bool inBonus;

	public void Init(int bonusLayer, int normalLayer, Action<float> addDistance, Action<Renderer, bool> setRendererToCamera, Func<float> getCurrentSpeed, Func<Vector3, bool> isBehindCamera, Func<Vector3, float> getPositionPercent)
	{
		this.bonusLayer = bonusLayer;
		this.normalLayer = normalLayer;

		AddDistance = addDistance;
		SetRendererToCamera = setRendererToCamera;
		GetCurrentSpeed = getCurrentSpeed;
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

		ManagePortals();
		ManageChunks();
	}

	void ManagePortals()
	{
		// move portals back
		List<Transform> toRemove = new List<Transform>();

		spawnedPortals.ForEach(item =>
		{
			if(item == null)
				toRemove.Add(item);
			else
			{
				float speed = inBonus ? currentSpeed : GetCurrentSpeed();
				item.Translate(0, 0, -speed * Time.deltaTime);
			}
		});

		// clean portal list
		toRemove.ForEach(item => spawnedPortals.Remove(item));
	}

	void ManageChunks()
	{
		// move chunks
		foreach (TerrainChunk chunk in spawnedChunks)
		{
			float speed = inBonus ? currentSpeed : GetCurrentSpeed();
			chunk.transform.Translate(0, 0, -speed * Time.deltaTime);
			AddDistance(speed * Time.deltaTime);
		}
	}

	void SpawnEmptyChunk(Vector3 position)
	{
		TerrainChunk emptyChunk = Instantiate(emptyChunkPrefab, position, Quaternion.identity, transform);
		emptyChunk.Init();

		spawnedChunks.Add(emptyChunk);
	}

	Portal SpawnPortal(Vector3 position, bool isEnd)
	{
		Portal portal = Instantiate(portalPrefab, position, Quaternion.identity);
		portal.Init(isEnd, SetRendererToCamera, () => { return GetPositionPercent(portal.transform.position); }, IsBehindCamera);

		spawnedPortals.Add(portal.transform);
		return portal;
	}

	public void SpawnTerrain(int portalLayer, float difficulty, float currentSpeed, Func<Vector3> GetLastChunkPos, Snake player)
	{
		if(!CheckInitialized())
			return;

		this.currentSpeed = currentSpeed;
		inBonus = false;

		Vector3 lastChunkPos = GetLastChunkPos();
		float chunkSize = emptyChunkPrefab.transform.GetChild(0).localScale.z;

		// spawn empty chunk to link with normal terrain
		SpawnEmptyChunk(lastChunkPos);

		// spawns start portal
		Transform[] playerPieces = player.GetPiecesTransforms();

		startPortal = SpawnPortal(lastChunkPos, false);
		startPortal.name = "StartPortal";
		startPortal.StartAnimation(player.transform, playerPieces);
		startPortal.SetLayer(portalLayer);

		// spawning empty chunk first
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
		endPortal = SpawnPortal(spawnPos, true);
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

		// make portal only visible inside bonus
		endPortal.SetLayer(bonusLayer);

		// link pieces
		for (int i = 0; i < 9; i++)
			copiedPieces[i].GetComponent<SnakePiece>().Init(copiedPieces[i + 1], 0);

		copiedPieces[copiedPieces.Length - 1].GetComponent<SnakePiece>().Init(copiedPieces[0], 0);
	}

	public void StopBonus()
	{
		if(!CheckInitialized())
			return;

		inBonus = false;

		spawnedChunks.ForEach(item => Destroy(item.gameObject));
		spawnedChunks.Clear();

		endPortal.SetLayer(normalLayer);
	}

	public void StartBonus()
	{
		if(!CheckInitialized())
			return;

		inBonus = true;
		startPortal.SetLayer(bonusLayer);
	}
}