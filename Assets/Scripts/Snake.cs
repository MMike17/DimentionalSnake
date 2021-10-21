using System.Collections.Generic;
using UnityEngine;

/// <summary>Class moving snake</summary>
public class Snake : BaseBehaviour
{
	[Header("Settings")]
	public int minSnakeLength;
	public float horizontalSpeed;
	public float zPieceDistanceFromCore, maxPieceXDistance;
	[Range(0, 1)]
	public float smoothingPercent, minSpeedRatio;

	[Header("Scene references")]
	public GameObject piecePrefab;

	List<GameObject> spawnedPieces;
	Vector3 targetPos;
	float minX, maxX, smoothMinX, smoothMaxX, currentSpeed;

	public void Init(float minX, float maxX)
	{
		this.minX = minX;
		this.maxX = maxX;

		spawnedPieces = new List<GameObject>();

		float range = maxX - minX;
		smoothMinX = minX + range * smoothingPercent;
		smoothMaxX = maxX - range * smoothingPercent;

		targetPos = transform.position;
		currentSpeed = 0;

		// spawning initial pieces
		Vector3 position = transform.position;

		for (int i = 0; i < minSnakeLength; i++)
		{
			position -= Vector3.forward * zPieceDistanceFromCore;
			GameObject snakePiece = Instantiate(piecePrefab, position, Quaternion.identity);

			spawnedPieces.Add(snakePiece);
		}

		InitInternal();
	}

	void Update()
	{
		if(!initialized)
			return;

		// move snake
		transform.position = Vector3.MoveTowards(transform.position, targetPos, currentSpeed * Time.deltaTime);

		ManagePieces();
	}

	void ManagePieces()
	{
		// TODO : Manage snake pieces
	}

	void SpawnSnakePiece()
	{
		Transform lastPiece = spawnedPieces[spawnedPieces.Count - 1].transform;

		float pieceZPos = lastPiece.position.z - zPieceDistanceFromCore;
		float xOffset = lastPiece.position.x - spawnedPieces[spawnedPieces.Count - 2].transform.position.x;

		Vector3 spawnPos = new Vector3(lastPiece.position.x + xOffset, 0, pieceZPos);

		GameObject spawnedPiece = Instantiate(piecePrefab, spawnPos, Quaternion.identity);
		spawnedPieces.Add(spawnedPiece);
	}

	void OnTriggerEnter(Collider other)
	{
		// TODO : Detect collisions with elements
	}

	public void SetXPosOnTerrain(float percent)
	{
		// set speed
		float smoothingRatio = 0;
		float distance = maxX - smoothMaxX;

		// smooth to the right
		if(transform.position.x >= smoothMaxX)
			smoothingRatio = Mathf.Lerp(1, minSpeedRatio, (transform.position.x - smoothMaxX) / distance);

		// smooth to the left
		if(transform.position.x <= smoothMinX)
			smoothingRatio = Mathf.Lerp(1, minSpeedRatio, 1 - (transform.position.x - minX) / distance);

		currentSpeed = horizontalSpeed * smoothingRatio;

		// pick target pos
		targetPos = transform.position;
		targetPos.x = Mathf.Lerp(minX, maxX, percent);
	}
}