using System;
using UnityEngine;

/// <summary>Manages all camera movements</summary>
public class CameraManager : BaseBehaviour
{
	public const int BONUS_RENDER_LAYER = 3;

	const int UI_RENDER_LAYER = 5;
	const int PLAYER_RENDER_LAYER = 6;

	[Header("Settings")]
	public float loseAnimDuration;
	public float introAnimDuration, maxIntroMagnitude;
	public AnimationCurve loseAnimCurve, introAnimCurve;
	public Vector3 offset;

	[Header("Scene references")]
	public Camera mainCamera;
	public Camera bonusCamera, extraCamera;

	Transform player;
	RenderTexture portalTexture, extrapPortalTexture;
	Vector3 initialPos, targetPos;
	Quaternion initialRot, targetRot;
	Action OnLoseAnimDone;
	float loseTimer, introTimer;
	bool startAnim, introAnim, isInBonus;

	void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;

		if(player != null)
			Gizmos.DrawLine(player.position, player.position + offset);
	}

	public void Init(Transform player)
	{
		this.player = player;

		initialPos = mainCamera.transform.position;
		initialRot = mainCamera.transform.rotation;

		portalTexture = new RenderTexture(Screen.width, Screen.height, 24);
		portalTexture.name = "PortalTexture";
		portalTexture.Create();

		extrapPortalTexture = new RenderTexture(Screen.width, Screen.height, 24);
		extrapPortalTexture.name = "ExtraPortalTexture";
		extrapPortalTexture.Create();

		extraCamera.targetTexture = extrapPortalTexture;

		loseTimer = 0;
		startAnim = false;
		isInBonus = false;
		introAnim = true;

		InitInternal();

		Reset();
	}

	void Update()
	{
		if(!initialized)
			return;

		if(introAnim)
		{
			introTimer += Time.deltaTime;

			mainCamera.transform.position = Vector3.Lerp(initialPos - Vector3.up * maxIntroMagnitude, initialPos, introAnimCurve.Evaluate(introTimer / introAnimDuration));

			if(introTimer >= introAnimDuration)
			{
				mainCamera.transform.position = initialPos;
				introAnim = false;
			}
		}

		if(startAnim)
		{
			loseTimer += Time.deltaTime;

			mainCamera.transform.position = Vector3.Lerp(initialPos, targetPos, loseAnimCurve.Evaluate(loseTimer / loseAnimDuration));
			mainCamera.transform.rotation = Quaternion.Lerp(initialRot, targetRot, loseAnimCurve.Evaluate(loseTimer / loseAnimDuration));

			if(loseTimer >= loseAnimDuration)
			{
				mainCamera.transform.position = targetPos;
				mainCamera.transform.rotation = targetRot;

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

		mainCamera.transform.position = initialPos;
		mainCamera.transform.rotation = initialRot;

		isInBonus = false;

		bonusCamera.cullingMask = (1 << BONUS_RENDER_LAYER);
		bonusCamera.targetTexture = portalTexture;

		mainCamera.targetTexture = null;
	}

	public void SetRendererToCamera(Renderer renderer, bool needsExtra)
	{
		if(!CheckInitialized())
			return;

		renderer.material.SetTexture("_MainTex", needsExtra ? extrapPortalTexture : portalTexture);
	}

	public void SwitchCamera()
	{
		if(!CheckInitialized())
			return;

		isInBonus = !isInBonus;

		if(isInBonus)
			bonusCamera.cullingMask = (1 << BONUS_RENDER_LAYER) | (1 << PLAYER_RENDER_LAYER) | (1 << UI_RENDER_LAYER);
		else
			bonusCamera.cullingMask = (1 << BONUS_RENDER_LAYER);

		bonusCamera.targetTexture = isInBonus ? null : portalTexture;
		mainCamera.targetTexture = isInBonus ? portalTexture : null;
	}
}