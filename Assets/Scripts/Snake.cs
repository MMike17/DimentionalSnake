using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Class moving snake</summary>
public class Snake : BaseBehaviour
{
	const string OBSTACLE_TAG = "Obstacle";
	const string MONEY_TAG = "Money";
	const string PIECE_TAG = "Piece";

	[Header("Settings")]
	public int minSnakeLength;
	public float horizontalSpeed;
	public float zPieceDistanceFromCore, maxPieceXDistance;
	[Range(0, 1)]
	public float smoothingPercent, minSpeedRatio, minPieceSpeedRatio;

	[Header("Scene references")]
	public GameObject piecePrefab;

	List<GameObject> spawnedPieces;
	Action GetMoney, LoseGame;
	Vector3 targetPos;
	float minX, maxX, smoothMinX, smoothMaxX, currentSpeed;

	void OnDrawGizmos()
	{
		if(spawnedPieces != null)
		{
			Vector3 previousPos = transform.position;

			spawnedPieces.ForEach(item =>
			{
				// decide color
				float distance = Mathf.Abs(previousPos.x - item.transform.position.x);

				if(distance >= maxPieceXDistance)
					Gizmos.color = Color.blue;
				else
					Gizmos.color = Color.green;

				SetGizmosAlpha(0.8f);

				// display lines
				Gizmos.DrawLine(item.transform.position - Vector3.right * maxPieceXDistance, item.transform.position - Vector3.right * maxPieceXDistance + Vector3.forward * zPieceDistanceFromCore);
				Gizmos.DrawLine(item.transform.position + Vector3.right * maxPieceXDistance, item.transform.position + Vector3.right * maxPieceXDistance + Vector3.forward * zPieceDistanceFromCore);

				previousPos = item.transform.position;
			});
		}
	}

	public void Init(float minX, float maxX, Action getMoney, Action loseGame)
	{
		this.minX = minX;
		this.maxX = maxX;
		GetMoney = getMoney;
		LoseGame = loseGame;

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
		Vector3 previousPos = transform.position;

		foreach (GameObject piece in spawnedPieces)
		{
			// decide movement amount and speed
			Vector3 target = piece.transform.position;
			target.x = previousPos.x;

			float distance = Mathf.Abs(previousPos.x - piece.transform.position.x);
			float smoothingRatio = 1;

			if(distance < maxPieceXDistance)
				smoothingRatio = Mathf.Lerp(1, minPieceSpeedRatio, distance / maxPieceXDistance);

			piece.transform.position = Vector3.MoveTowards(piece.transform.position, target, horizontalSpeed * smoothingRatio * Time.deltaTime);

			// rotate piece
			piece.transform.LookAt(previousPos, Vector3.up);

			previousPos = piece.transform.position;
		}
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
		switch(other.tag)
		{
			case OBSTACLE_TAG:
				LoseGame();
				break;

			case MONEY_TAG:
				Destroy(other.gameObject);
				GetMoney();
				break;

			case PIECE_TAG:
				Destroy(other.gameObject);
				SpawnSnakePiece();
				break;
		}
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