using UnityEngine;

/// <summary>Class moving snake</summary>
public class Snake : BaseBehaviour
{
	// TODO : Manage snake pieces
	// TODO : Spawn snake pieces
	// TODO : Detect collisions with elements

	[Header("Settings")]
	public float horizontalSpeed;
	[Range(0, 1)]
	public float smoothingPercent, minSpeedRatio;

	Vector3 targetPos;
	float minX, maxX, smoothMinX, smoothMaxX, currentSpeed;

	public void Init(float minX, float maxX)
	{
		this.minX = minX;
		this.maxX = maxX;

		float range = maxX - minX;
		smoothMinX = minX + range * smoothingPercent;
		smoothMaxX = maxX - range * smoothingPercent;

		targetPos = transform.position;
		currentSpeed = 0;

		InitInternal();
	}

	void Update()
	{
		if(!initialized)
			return;

		// move snake
		transform.position = Vector3.MoveTowards(transform.position, targetPos, currentSpeed * Time.deltaTime);
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