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

	// TODO : disable smoothing when not going towards the edge

	[Header("Settings")]
	public int minSnakeLength;
	public float horizontalSpeed;
	public float zPieceDistanceFromCore;
	[Range(0, 1)]
	public float smoothingPercent, minSpeedRatio, headAlignmentPercent;

	[Header("Scene references")]
	public GameObject piecePrefab;
	public Transform head;

	List<Transform> spawnedPieces;
	List<ReferencePoint> referencePoints;
	Func<float> GetCurrentSpeed;
	Action GetMoney, LoseGame;
	Vector3 targetPos;
	float minX, maxX, smoothMinX, smoothMaxX, currentSpeed;

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

		spawnedPieces = new List<Transform>();
		referencePoints = new List<ReferencePoint>();

		float range = maxX - minX;
		smoothMinX = minX + range * smoothingPercent / 2;
		smoothMaxX = maxX - range * smoothingPercent / 2;

		targetPos = transform.position;
		currentSpeed = 0;

		// spawning initial pieces
		Vector3 position = transform.position;

		for (int i = 0; i < minSnakeLength; i++)
		{
			position -= Vector3.forward * zPieceDistanceFromCore;
			GameObject snakePiece = Instantiate(piecePrefab, position, Quaternion.identity);

			spawnedPieces.Add(snakePiece.transform);
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
		// add new point and move it depending on the current speed
		referencePoints.Insert(0, new ReferencePoint(head.position));
		referencePoints.ForEach(item => item.Move(GetCurrentSpeed()));

		ReferencePoint previousPoint, nextpoint;
		Vector3 previousPos = head.position;

		for (int i = 0; i < spawnedPieces.Count; i++)
		{
			Transform piece = spawnedPieces[i];

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
			Vector3 nextOffset = head.position - piece.position;

			if(i < spawnedPieces.Count - 1)
				previousOffset = piece.position - spawnedPieces[i + 1].position;

			if(i > 0)
				nextOffset = spawnedPieces[i - 1].position - piece.position;

			piece.LookAt(piece.position + Vector3.Lerp(previousOffset, nextOffset, 0.5f));

			previousPos = piece.position;
		}

		// orient head
		Vector3 offset = Vector3.Normalize(head.position - spawnedPieces[0].transform.position);
		head.LookAt(head.position + Vector3.Lerp(Vector3.forward, offset, headAlignmentPercent));
	}

	void SpawnSnakePiece()
	{
		Transform lastPiece = spawnedPieces[spawnedPieces.Count - 1].transform;

		float pieceZPos = lastPiece.position.z - zPieceDistanceFromCore;
		float xOffset = lastPiece.position.x - spawnedPieces[spawnedPieces.Count - 2].transform.position.x;

		Vector3 spawnPos = new Vector3(lastPiece.position.x + xOffset, transform.position.y, pieceZPos);

		GameObject spawnedPiece = Instantiate(piecePrefab, spawnPos, Quaternion.identity);
		spawnedPieces.Add(spawnedPiece.transform);
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