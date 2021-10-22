using System;
using UnityEngine;

/// <summary>Manages all camera movements</summary>
public class CameraManager : BaseBehaviour
{
	[Header("Settings")]
	public float animationDuration;
	public AnimationCurve loseAnimCurve;
	public Vector3 offset;

	[Header("Scene references")]
	public Transform mainCamera;

	Transform player;
	Vector3 initialPos, targetPos;
	Quaternion initialRot, targetRot;
	Action OnLoseAnimDone;
	float timer;
	bool startAnim;

	void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;

		if(player != null)
			Gizmos.DrawLine(player.position, player.position + offset);
	}

	public void Init(Transform player)
	{
		this.player = player;

		initialPos = mainCamera.position;
		initialRot = mainCamera.rotation;

		timer = 0;
		startAnim = false;

		InitInternal();
	}

	void Update()
	{
		if(!initialized)
			return;

		if(startAnim)
		{
			timer += Time.deltaTime;

			mainCamera.position = Vector3.Lerp(initialPos, targetPos, loseAnimCurve.Evaluate(timer / animationDuration));
			mainCamera.rotation = Quaternion.Lerp(initialRot, targetRot, loseAnimCurve.Evaluate(timer / animationDuration));

			if(timer >= animationDuration)
			{
				mainCamera.position = targetPos;
				mainCamera.rotation = targetRot;

				startAnim = false;
				OnLoseAnimDone();
			}
		}
	}

	public void StartLoseAnimation(Action onLoseAnimDone)
	{
		if(!CheckInitialized())
			return;

		OnLoseAnimDone = onLoseAnimDone;

		targetPos = player.position + offset;
		targetRot = Quaternion.LookRotation(player.position - targetPos, Vector3.up);

		startAnim = true;
		timer = 0;
	}

	public void Reset()
	{
		if(!CheckInitialized())
			return;

		mainCamera.position = initialPos;
		mainCamera.rotation = initialRot;
	}
}