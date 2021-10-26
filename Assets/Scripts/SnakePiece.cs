using System.Collections.Generic;
using UnityEngine;

/// <summary>Manages rendereing of lines between snake pieces</summary>
[RequireComponent(typeof(LineRenderer))]
public class SnakePiece : BaseBehaviour
{
	[Header("Settings")]
	public float zOffset;

	Vector3 initialPos => transform.position - transform.forward * zOffset;

	List<Vector3> lastPositions;
	LineRenderer lineRenderer;
	Transform target;
	int trailLength;
	bool movementAnimations;

	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.cyan;

		if(target != null)
			Gizmos.DrawLine(initialPos, target.position);
	}

	public void Init(Transform target, int trailLength)
	{
		this.target = target;
		this.trailLength = trailLength;

		lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.positionCount = 2;

		movementAnimations = false;

		InitInternal();
	}

	void Update()
	{
		if(!initialized)
			return;

		if(movementAnimations)
		{
			// TODO : May have to reverse this

			lastPositions.Add(initialPos);

			if(lastPositions.Count > trailLength)
				lastPositions.RemoveAt(0);

			lineRenderer.SetPositions(lastPositions.ToArray());
		}
		else
		{
			Vector3[] points;
			lineRenderer.positionCount = 2;

			if(target != null)
				points = new Vector3[2] { initialPos, target.position };
			else
				points = new Vector3[2] { initialPos, initialPos - transform.forward * 2 };

			lineRenderer.SetPositions(points);
		}
	}

	public void StartTrailAnimation()
	{
		if(!CheckInitialized())
			return;

		lineRenderer.positionCount = trailLength;
		movementAnimations = true;

		lastPositions = new List<Vector3>();

		for (int i = 0; i < trailLength; i++)
			lastPositions.Add(initialPos);
	}
}