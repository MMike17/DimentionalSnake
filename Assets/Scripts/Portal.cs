using System;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

/// <summary>Manages the features and animations of the portal</summary>
public class Portal : BaseBehaviour
{
	[Header("Settings")]
	public float circleRadius;

	[Header("First anim")]
	public float minAnimRadius;
	public float maxAnimRadius, firstPartAnimDuration;
	public AnimationCurve firstPartAnimCurve;

	[Header("Second anim")]
	public float secondPartAnimDuration;
	public AnimationCurve secondPartAnimCurve;

	[Header("Third anim")]
	public float thirdPartAnimDuration;
	public AnimationCurve thirdPartAnimCurve;

	[Header("Scene references")]
	public Transform portalCenter;
	public MeshRenderer renderInsidePortal, renderOutsidePortal;
	public Transform[] targetPositions;

	Func<Vector3, bool> IsBehindCamera;
	Vector2[] targetOffsets;
	Vector3 animOffset;

	void OnDrawGizmos()
	{
#if UNITY_EDITOR
		Handles.color = Color.grey;
		Handles.DrawWireDisc(transform.GetChild(0).position, Vector3.forward, circleRadius);

		if(portalCenter != null)
		{
			Handles.DrawWireArc(portalCenter.position - Vector3.forward, Vector3.forward, Vector3.right, 180, minAnimRadius);
			Handles.DrawWireArc(portalCenter.position - Vector3.forward * 2, Vector3.forward, Vector3.right, 180, maxAnimRadius);
		}
#endif

		Gizmos.color = Color.grey;

		if(targetPositions != null && targetPositions.Length > 0)
		{
			for (int i = 0; i < targetPositions.Length - 1; i++)
				Gizmos.DrawLine(targetPositions[i].position, targetPositions[i + 1].position);

			if(targetPositions.Length > 1)
				Gizmos.DrawLine(targetPositions[0].position, targetPositions[targetPositions.Length - 1].position);

			if(portalCenter != null)
			{
				foreach (Transform target in targetPositions)
					Gizmos.DrawLine(target.position, portalCenter.position);
			}
		}
	}

	public void Init(Action<Renderer> SetRenderCamera, Func<Vector3, bool> isBehindCamera)
	{
		IsBehindCamera = isBehindCamera;

		SetRenderCamera(renderInsidePortal);

		InitInternal();
	}

	void Update()
	{
		if(!initialized)
			return;

		if(IsBehindCamera(transform.position))
			Destroy(gameObject);
	}

	IEnumerator AnimatePiecesRoutine(Transform player, Transform[] pieces)
	{
		yield return FirstPartAnimRoutine(player, pieces);
		yield return SecondPartAnimRoutine(player, pieces);
		yield return ThirdPartAnimRoutine();
	}

	IEnumerator FirstPartAnimRoutine(Transform player, Transform[] pieces)
	{
		// decide pieces offsets
		targetOffsets = new Vector2[10];

		for (int i = 0; i < 10; i++)
		{
			int side = UnityEngine.Random.value >= 0.5f ? 1 : -1;
			Vector2 offset = Vector3.Normalize(UnityEngine.Random.value * Vector3.up + UnityEngine.Random.value * Vector3.right * side);

			targetOffsets[i] = offset * Mathf.Lerp(minAnimRadius, maxAnimRadius, UnityEngine.Random.value);
		}

		// animate pieces
		float timer = 0;

		while (timer <= firstPartAnimDuration)
		{
			for (int i = 0; i < pieces.Length; i++)
			{
				Transform piece = pieces[i];
				Vector2 offset = targetOffsets[i];
				float currentPercent = firstPartAnimCurve.Evaluate(timer / firstPartAnimDuration);

				Vector3 initialPosition = new Vector3(player.position.x, player.position.y, piece.position.z);
				Vector3 targetPos = new Vector3(player.position.x + offset.x, player.position.y + offset.y, piece.position.z);

				piece.rotation = Quaternion.Lerp(player.rotation, Quaternion.LookRotation(targetPositions[i].position - piece.position), currentPercent);
				piece.position = Vector3.Lerp(initialPosition, targetPos, currentPercent);
			}

			timer += Time.deltaTime;
			yield return null;
		}
	}

	IEnumerator SecondPartAnimRoutine(Transform player, Transform[] pieces)
	{
		// move pieces to portal
		float timer = 0;

		while (timer <= secondPartAnimDuration)
		{
			for (int i = 0; i < 10; i++)
			{
				Transform piece = pieces[i];
				Vector3 initialPosition = new Vector3(player.position.x, player.position.y, piece.position.z);
				float currentPercent = secondPartAnimCurve.Evaluate(timer / secondPartAnimDuration);

				piece.position = Vector3.Lerp(initialPosition, targetPositions[i].position, currentPercent);

				// away
				if(timer < secondPartAnimDuration * 2 / 3)
				{
					piece.rotation = Quaternion.Lerp(player.rotation, Quaternion.LookRotation(targetPositions[i].position - piece.position), currentPercent);
				}
				else // close
				{
					Vector3 direction = Quaternion.Euler(0, -90, 0) * (targetPositions[i].position - portalCenter.position);
					piece.rotation = Quaternion.Lerp(Quaternion.LookRotation(targetPositions[i].position - piece.position), Quaternion.LookRotation(direction), currentPercent);
				}
			}

			timer += Time.deltaTime;
			yield return null;
		}

		// links snake pieces
		for (int i = 0; i < 9; i++)
			pieces[i].GetComponent<SnakePiece>().Init(pieces[i + 1], 0);

		pieces[pieces.Length - 1].GetComponent<SnakePiece>().Init(pieces[0], 0);
	}

	IEnumerator ThirdPartAnimRoutine()
	{
		float timer = 0;

		// animate portal mesh
		while (timer <= thirdPartAnimDuration)
		{
			renderInsidePortal.transform.localScale = Vector3.one * thirdPartAnimCurve.Evaluate(timer / thirdPartAnimDuration) * circleRadius;

			timer += Time.deltaTime;
			yield return null;
		}
	}

	public void StartAnimation(Transform player, Transform[] pieces)
	{
		if(!CheckInitialized())
			return;

		StartCoroutine(AnimatePiecesRoutine(player, pieces));

		renderInsidePortal.transform.localScale = Vector3.zero;
		renderInsidePortal.gameObject.SetActive(true);
		renderOutsidePortal.gameObject.SetActive(false);
	}

	public void SwitchWorlds()
	{
		if(!CheckInitialized())
			return;

		renderInsidePortal.gameObject.SetActive(false);
		renderOutsidePortal.gameObject.SetActive(true);
	}
}