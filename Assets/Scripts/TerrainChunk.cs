using System.Collections.Generic;
using UnityEngine;

/// <summary>Manages the difficulty and modulation of a terrain chunk</summary>
public class TerrainChunk : BaseBehaviour
{
	[Header("Scene references")]
	public GameObject[] obstacles;
	public GameObject[] pieces;

	List<Rigidbody> elements;
	float upForce, sideForce, destroyDelay;
	bool triggeredSpawnNew;

	public void Init(float difficulty = 0, float upForce = 0, float sideForce = 0, float destroyDelay = 0)
	{
		this.upForce = upForce;
		this.sideForce = sideForce;
		this.destroyDelay = destroyDelay;

		triggeredSpawnNew = false;
		List<int> selectedIndexes;

		// decide which obstacles need to be spawned
		if(obstacles.Length != 0)
		{
			int obstaclesDifficulty = GetObstaclesDifficulty(difficulty);
			selectedIndexes = GetSelectedIndexes(obstacles.Length, obstaclesDifficulty);

			for (int i = 0; i < obstacles.Length; i++)
				obstacles[i].SetActive(selectedIndexes.Contains(i));
		}

		// decide which pieces need to be spawned
		if(pieces.Length != 0)
		{
			int piecesDifficulty = GetPiecesDifficulty(difficulty);
			selectedIndexes = GetSelectedIndexes(pieces.Length, piecesDifficulty);

			for (int i = 0; i < pieces.Length; i++)
				pieces[i].SetActive(selectedIndexes.Contains(i));
		}

		elements = new List<Rigidbody>();
		GetComponentsRecursive(transform);

		InitInternal();
	}

	void GetComponentsRecursive(Transform parent)
	{
		Rigidbody rigid = parent.GetComponent<Rigidbody>();

		if(rigid != null)
			elements.Add(rigid);

		foreach (Transform child in parent)
			GetComponentsRecursive(child);
	}

	int GetObstaclesDifficulty(float difficulty)
	{
		return Mathf.FloorToInt(difficulty * obstacles.Length);
	}

	int GetPiecesDifficulty(float difficulty)
	{
		return Mathf.CeilToInt(difficulty * pieces.Length);
	}

	List<int> GetSelectedIndexes(int arrayCount, int pickCount)
	{
		if(arrayCount == 0)
			return new List<int>();

		List<int> selectedIndexes = new List<int>();
		List<int> availableIndexes = new List<int>();

		for (int i = 0; i < arrayCount; i++)
			availableIndexes.Add(i);

		for (int i = 0; i < pickCount; i++)
		{
			Debug.Log(availableIndexes.Count);
			int selectedIndex = availableIndexes[Random.Range(0, availableIndexes.Count)];

			selectedIndexes.Add(selectedIndex);
			availableIndexes.Remove(selectedIndex);
		}

		return selectedIndexes;
	}

	public bool ShouldSpawnNew(float playerZPos)
	{
		if(triggeredSpawnNew)
			return false;
		else if(transform.position.z <= playerZPos)
		{
			triggeredSpawnNew = true;
			return true;
		}
		else
			return false;
	}

	public void PopObstacles()
	{
		foreach (Rigidbody body in elements)
		{
			// skip if null
			if(body == null)
				continue;

			body.useGravity = true;
			body.isKinematic = false;

			Vector3 force = Vector3.up * upForce;
			int side = body.transform.position.x >= transform.position.x ? 1 : -1;
			force += Vector3.right * side * sideForce;

			body.AddForce(force, ForceMode.Impulse);
			body.AddTorque(new Vector3(Random.value * sideForce, Random.value * side * sideForce / 2, Random.value * upForce), ForceMode.Impulse);
		}

		// destroy after delay
		DelayedActionsManager.SceduleAction(() =>
		{
			foreach (Rigidbody body in elements)
			{
				if(body != null)
					Destroy(body.gameObject);
			}
		}, destroyDelay);
	}
}