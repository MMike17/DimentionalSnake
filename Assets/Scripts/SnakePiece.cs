using UnityEngine;

/// <summary>Manages rendereing of lines between snake pieces</summary>
[RequireComponent(typeof(LineRenderer))]
public class SnakePiece : BaseBehaviour
{
	[Header("Settings")]
	public float zOffset;

	Vector3 initialPos => transform.position - transform.forward * zOffset;

	LineRenderer lineRenderer;
	Transform target;

	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.cyan;

		if(target != null)
			Gizmos.DrawLine(initialPos, target.position);
	}

	public void Init(Transform target)
	{
		this.target = target;

		lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.positionCount = 2;

		InitInternal();
	}

	void Update()
	{
		if(!initialized)
			return;

		Vector3[] points;
		lineRenderer.positionCount = 2;

		if(target != null)
			points = new Vector3[2] { initialPos, target.position };
		else
			points = new Vector3[2] { initialPos, initialPos - transform.forward * 2 };

		lineRenderer.SetPositions(points);
	}

	// TODO : Add animation movement
}