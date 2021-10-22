using System;
using UnityEngine;

/// <summary>Manages all camera movements</summary>
public class CameraManager : BaseBehaviour
{
	[Header("Settings")]
	public float loseAnimDuration;
	public float introAnimDuration, maxIntroMagnitude;
	public AnimationCurve loseAnimCurve, introAnimCurve;
	public Vector3 offset;

	[Header("Scene references")]
	public Transform mainCamera;

	Transform player;
	Vector3 initialPos, targetPos;
	Quaternion initialRot, targetRot;
	Action OnLoseAnimDone;
	float loseTimer, introTimer;
	bool startAnim, introAnim;

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

		loseTimer = 0;
		startAnim = false;
		introAnim = true;

		InitInternal();
	}

	void Update()
	{
		if(!initialized)
			return;

		if(introAnim)
		{
			introTimer += Time.deltaTime;

			mainCamera.position = Vector3.Lerp(initialPos - Vector3.up * maxIntroMagnitude, initialPos, introAnimCurve.Evaluate(introTimer / introAnimDuration));

			if(introTimer >= introAnimDuration)
			{
				mainCamera.position = initialPos;
				introAnim = false;
			}
		}

		if(startAnim)
		{
			loseTimer += Time.deltaTime;

			mainCamera.position = Vector3.Lerp(initialPos, targetPos, loseAnimCurve.Evaluate(loseTimer / loseAnimDuration));
			mainCamera.rotation = Quaternion.Lerp(initialRot, targetRot, loseAnimCurve.Evaluate(loseTimer / loseAnimDuration));

			if(loseTimer >= loseAnimDuration)
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
		loseTimer = 0;
	}

	public void Reset()
	{
		if(!CheckInitialized())
			return;

		mainCamera.position = initialPos;
		mainCamera.rotation = initialRot;
	}
}