using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Class moving snake</summary>
[RequireComponent(typeof(Rigidbody))]
public class Snake : BaseBehaviour
{
	const string OBSTACLE_TAG = "Obstacle";
	const string MONEY_TAG = "Money";
	const string PIECE_TAG = "Piece";

	[Header("Settings")]
	public int minSnakeLength;
	public float horizontalSpeed;
	public float zPieceDistanceFromCore;
	[Range(0, 1)]
	public float smoothingPercent, minSpeedRatio, headAlignmentPercent;

	[Header("Scene references")]
	public SnakePiece piecePrefab;
	public SnakePiece head;

	List<SnakePiece> spawnedPieces;
	List<ReferencePoint> referencePoints;
	Func<float> GetCurrentSpeed;
	Action GetMoney, LoseGame;
	Vector3 initialPos, targetPos;
	float minX, maxX, smoothMinX, smoothMaxX, currentSpeed;
	bool canMove;

	void OnDrawGizmos()
	{
		Gizmos.color = Color.green;

		if(spawnedPieces != null && spawnedPieces.Count > 0)
		{
			Gizmos.DrawLine(Vector3.up * 0.5f + Vector3.right * smoothMinX - Vector3.forward * 5, Vector3.up * 0.5f + Vector3.right * smoothMinX + Vector3.forward * 5);
			Gizmos.DrawLine(Vector3.up * 0.5f + Vector3.right * smoothMaxX - Vector3.forward * 5, Vector3.up * 0.5f + Vector3.right * smoothMaxX + Vector3.forward * 5);
		}
	}

	public void Init(float minX, float maxX, Action getMoney, Action loseGame, Func<float> getCurrentSpeed)
	{
		this.minX = minX;
		this.maxX = maxX;
		GetMoney = getMoney;
		LoseGame = loseGame;
		GetCurrentSpeed = getCurrentSpeed;

		spawnedPieces = new List<SnakePiece>();
		referencePoints = new List<ReferencePoint>();

		initialPos = transform.position;

		float range = maxX - minX;
		smoothMinX = minX + range * smoothingPercent / 2;
		smoothMaxX = maxX - range * smoothingPercent / 2;

		targetPos = transform.position;
		currentSpeed = 0;
		canMove = false;

		InitInternal();

		// spawning initial pieces
		Reset();
	}

	void Update()
	{
		if(!initialized || !canMove)
			return;

		// move snake
		transform.position = Vector3.MoveTowards(transform.position, targetPos, currentSpeed * Time.deltaTime);

		ManagePieces();
		CleanList();
	}

	void ManagePieces()
	{
		// add new point and move it depending on the current speed
		referencePoints.Insert(0, new ReferencePoint(head.transform.position));
		referencePoints.ForEach(item => item.Move(GetCurrentSpeed()));

		ReferencePoint previousPoint, nextpoint;
		Vector3 previousPos = head.transform.position;

		for (int i = 0; i < spawnedPieces.Count; i++)
		{
			Transform piece = spawnedPieces[i].transform;

			// pick the two nearest points on z axis
			int firstAfterIndex = -1;

			for (int j = 0; j < referencePoints.Count; j++)
			{
				if(referencePoints[j].zPos < piece.position.z)
				{
					firstAfterIndex = j;
					break;
				}
			}

			// skip the positionning part if not enough points yet
			if(firstAfterIndex > 0)
			{
				previousPoint = referencePoints[firstAfterIndex];
				nextpoint = referencePoints[firstAfterIndex - 1];

				float percent = (previousPoint.zPos + piece.position.z) / (nextpoint.zPos - previousPoint.zPos);

				// lerp between previous and next points
				piece.position = new Vector3(Mathf.Lerp(previousPoint.xPos, nextpoint.xPos, percent), piece.position.y, piece.position.z);
			}

			// orient  piece
			Vector3 previousOffset = Vector3.forward;
			Vector3 nextOffset = head.transform.position - piece.position;

			if(i < spawnedPieces.Count - 1)
				previousOffset = piece.position - spawnedPieces[i + 1].transform.position;

			if(i > 0)
				nextOffset = spawnedPieces[i - 1].transform.position - piece.position;

			piece.LookAt(piece.position + Vector3.Lerp(previousOffset, nextOffset, 0.5f));

			previousPos = piece.position;
		}

		// orient head
		Vector3 offset = Vector3.Normalize(head.transform.position - spawnedPieces[0].transform.position);
		head.transform.LookAt(head.transform.position + Vector3.Lerp(Vector3.forward, offset, headAlignmentPercent));
	}

	void CleanList()
	{
		float tipOfTail = spawnedPieces[spawnedPieces.Count - 1].transform.position.z;
		List<ReferencePoint> toRemove = new List<ReferencePoint>();

		referencePoints.ForEach(item =>
		{
			if(item.zPos <= tipOfTail)
				toRemove.Add(item);
		});

		toRemove.ForEach(item => referencePoints.Remove(item));
	}

	void SpawnSnakePiece()
	{
		Transform lastPiece = spawnedPieces[spawnedPieces.Count - 1].transform;

		float pieceZPos = lastPiece.position.z - zPieceDistanceFromCore;
		float xOffset = lastPiece.position.x - spawnedPieces[spawnedPieces.Count - 2].transform.position.x;

		Vector3 spawnPos = new Vector3(lastPiece.position.x + xOffset, transform.position.y, pieceZPos);

		SnakePiece spawnedPiece = Instantiate(piecePrefab, spawnPos, Quaternion.identity);

		spawnedPieces[spawnedPieces.Count - 1].Init(spawnedPiece.transform);
		spawnedPiece.Init(null);

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
		// pick target pos
		targetPos = transform.position;
		targetPos.x = Mathf.Lerp(minX, maxX, percent);

		// set speed
		float smoothingRatio = 1;
		float distance = maxX - smoothMaxX;

		// smooth to the right when going right
		if(transform.position.x >= smoothMaxX && transform.position.x < targetPos.x)
			smoothingRatio = Mathf.Lerp(1, minSpeedRatio, (transform.position.x - smoothMaxX) / distance);

		// smooth to the left when going left
		if(transform.position.x <= smoothMinX && transform.position.x > targetPos.x)
			smoothingRatio = Mathf.Lerp(1, minSpeedRatio, 1 - (transform.position.x - minX) / distance);

		currentSpeed = horizontalSpeed * smoothingRatio;
	}

	public void Freeze()
	{
		if(!CheckInitialized())
			return;

		canMove = false;
	}

	public void Reset()
	{
		if(!CheckInitialized())
			return;

		// clean list
		spawnedPieces.ForEach(item => Destroy(item.gameObject));
		spawnedPieces.Clear();

		// reset pos
		transform.position = initialPos;

		// spawn new parts
		Vector3 position = transform.position;

		for (int i = 0; i < minSnakeLength; i++)
		{
			position -= Vector3.forward * zPieceDistanceFromCore;
			SnakePiece snakePiece = Instantiate(piecePrefab, position, Quaternion.identity);

			spawnedPieces.Add(snakePiece);
		}

		// configure pieces
		head.Init(spawnedPieces[0].transform);

		for (int i = 0; i < spawnedPieces.Count; i++)
		{
			if(i + 1 != spawnedPieces.Count)
				spawnedPieces[i].Init(spawnedPieces[i + 1].transform);
			else
				spawnedPieces[i].Init(null);
		}
	}

	public void Unfreeze()
	{
		if(!CheckInitialized())
			return;

		Reset();
		canMove = true;

		targetPos = Vector3.zero;
	}

	class ReferencePoint
	{
		public float xPos => position.x;
		public float zPos => position.z;

		Vector3 position;

		public ReferencePoint(Vector3 pos)
		{
			position = pos;
		}

		public void Move(float speed)
		{
			position -= Vector3.forward * speed * Time.deltaTime;
		}
	}
}