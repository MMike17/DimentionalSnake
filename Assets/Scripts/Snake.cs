using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Class moving snake</summary>
[RequireComponent(typeof(Rigidbody))]
public class Snake : BaseBehaviour
{
	// TODO : Fix head angling (very weird)

	const string OBSTACLE_TAG = "Obstacle";
	const string MONEY_TAG = "Money";
	const string PIECE_TAG = "Piece";
	const string GROUND_TAG = "Ground";

	[Header("Settings")]
	public int minSnakeLength;
	public float horizontalSpeed, zPieceDistanceFromCore, fallZPos;
	[Range(0, 1)]
	public float smoothingPercent, minSpeedRatio, headAlignmentPercent;
	public Vector3 checkGroundBoxSize;

	[Header("Scene references")]
	public SnakePiece piecePrefab;
	public SnakePiece head;
	public Transform checkGroundBoxPos;

	List<SnakePiece> spawnedPieces;
	List<ReferencePoint> referencePoints;
	Func<float> GetCurrentSpeed;
	Rigidbody rigid;
	Action GetMoney, LoseGame, LoseGameByFall;
	Vector3 initialPos, targetPos;
	float minX, maxX, smoothMinX, smoothMaxX, currentSpeed;
	bool canMove, shouldFall;

	// TODO : Wire LoseGameByFall in

	void OnDrawGizmos()
	{
		Gizmos.color = Color.green;

		if(spawnedPieces != null && spawnedPieces.Count > 0)
		{
			SetGizmosAlpha(1);

			Gizmos.DrawLine(Vector3.up * 0.5f + Vector3.right * smoothMinX - Vector3.forward * 5, Vector3.up * 0.5f + Vector3.right * smoothMinX + Vector3.forward * 5);
			Gizmos.DrawLine(Vector3.up * 0.5f + Vector3.right * smoothMaxX - Vector3.forward * 5, Vector3.up * 0.5f + Vector3.right * smoothMaxX + Vector3.forward * 5);
		}

		if(checkGroundBoxPos != null)
		{
			SetGizmosAlpha(0.5f);
			Gizmos.DrawCube(checkGroundBoxPos.position, checkGroundBoxSize);
		}
	}

	public void Init(float minX, float maxX, Action getMoney, Action loseGame, Action loseGameByFall, Func<float> getCurrentSpeed)
	{
		this.minX = minX;
		this.maxX = maxX;
		GetMoney = getMoney;
		LoseGame = loseGame;
		LoseGameByFall = loseGameByFall;
		GetCurrentSpeed = getCurrentSpeed;

		spawnedPieces = new List<SnakePiece>();
		referencePoints = new List<ReferencePoint>();

		rigid = GetComponent<Rigidbody>();

		initialPos = transform.position;

		float range = maxX - minX;
		smoothMinX = minX + range * smoothingPercent / 2;
		smoothMaxX = maxX - range * smoothingPercent / 2;

		targetPos = transform.position;
		currentSpeed = 0;
		canMove = false;
		shouldFall = false;

		InitInternal();

		// spawning initial pieces
		Reset();
	}

	void Update()
	{
		if(!initialized || !canMove)
			return;

		ManagePieces();

		if(shouldFall)
		{
			if(CheckFallDone())
			{
				rigid.isKinematic = true;
				LoseGameByFall();
			}

			return;
		}

		// move snake
		transform.position = Vector3.MoveTowards(transform.position, targetPos, currentSpeed * Time.deltaTime);

		CheckFalling();
		CleanList();
	}

	bool CheckFallDone()
	{
		if(spawnedPieces[spawnedPieces.Count - 1].transform.position.y <= fallZPos)
			return true;
		else
			return false;
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
				if(referencePoints[j].position.z < piece.position.z)
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

				float percent = (previousPoint.position.z + piece.position.z) / (nextpoint.position.z - previousPoint.position.z);

				// lerp between previous and next points
				piece.position = new Vector3(
					Mathf.Lerp(previousPoint.position.x, nextpoint.position.x, percent),
					Mathf.Lerp(previousPoint.position.y, nextpoint.position.y, percent),
					piece.position.z);
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

	void CheckFalling()
	{
		Collider[] contacts = Physics.OverlapBox(checkGroundBoxPos.position, checkGroundBoxSize / 2);

		// short circuit's the checking method for performance optimization
		if(contacts.Length == 0)
		{
			shouldFall = true;
			rigid.useGravity = true;
			return;
		}

		bool hasGround = false;

		foreach (Collider detected in contacts)
		{
			if(detected.CompareTag(GROUND_TAG))
			{
				hasGround = true;
				break;
			}
		}

		if(!hasGround)
		{
			shouldFall = true;
			rigid.useGravity = true;
		}
	}

	void CleanList()
	{
		float tipOfTail = spawnedPieces[spawnedPieces.Count - 1].transform.position.z;
		List<ReferencePoint> toRemove = new List<ReferencePoint>();

		referencePoints.ForEach(item =>
		{
			if(item.position.z <= tipOfTail)
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
		rigid.isKinematic = true;
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
		head.transform.rotation = Quaternion.Euler(0, 0, 0);

		// configure snake
		rigid.useGravity = false;
		rigid.isKinematic = false;

		shouldFall = false;
		SetXPosOnTerrain(0.5f);

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

		// Clean list of references points
		referencePoints.Clear();
	}

	public void Unfreeze()
	{
		if(!CheckInitialized())
			return;

		Reset();

		canMove = true;
		rigid.isKinematic = false;

		targetPos = Vector3.zero;
	}

	class ReferencePoint
	{
		public Vector3 position { get; private set; }

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