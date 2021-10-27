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
	public float maxAnimRadius;
	[Range(0, 1)]
	public float firstAnimPercentDuration;
	public AnimationCurve firstPartAnimCurve;

	[Header("Second anim")]
	[Range(0, 1)]
	public float secondAnimPercentDuration;
	public AnimationCurve secondPartAnimCurve;

	[Header("Third anim")]
	[Range(0, 1)]
	public float thirdAnimPercentDuration;
	public AnimationCurve thirdPartAnimCurve;

	[Header("Scene references")]
	public Transform portalCenter;
	public MeshRenderer renderInsidePortal, renderOutsidePortal;
	public Transform[] targetPositions;

	Func<Vector3, bool> IsBehindCamera;
	Func<float> GetPercent;
	Vector3[] targetOffsets;
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
		SetGizmosAlpha(0.5f);

		if(targetPositions != null && targetPositions.Length > 0)
		{
			foreach (Transform target in targetPositions)
				Gizmos.DrawSphere(target.position, 0.5f);
		}

		if(firstAnimPercentDuration + secondAnimPercentDuration + thirdAnimPercentDuration > 1)
			thirdAnimPercentDuration = 1 - firstAnimPercentDuration - secondAnimPercentDuration;
	}

	public void Init(Action<Renderer> SetRenderCamera, Func<float> getPercent, Func<Vector3, bool> isBehindCamera)
	{
		GetPercent = getPercent;
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
		targetOffsets = new Vector3[10];

		for (int i = 0; i < 10; i++)
		{
			int side = UnityEngine.Random.value >= 0.5f ? 1 : -1;
			Vector2 offset = Vector3.Normalize(UnityEngine.Random.value * Vector3.up + UnityEngine.Random.value * Vector3.right * side);

			targetOffsets[i] = offset * Mathf.Lerp(minAnimRadius, maxAnimRadius, UnityEngine.Random.value);
		}

		// animate pieces
		while (GetPercent() < firstAnimPercentDuration)
		{
			float firstAnimPercent = GetPercent() / firstAnimPercentDuration;

			for (int i = 0; i < pieces.Length; i++)
			{
				Transform piece = pieces[i];
				Vector2 offset = targetOffsets[i];
				float currentPercent = firstPartAnimCurve.Evaluate(firstAnimPercent);

				Vector3 initialPosition = new Vector3(player.position.x, player.position.y, piece.position.z);
				Vector3 targetPos = new Vector3(player.position.x + offset.x, player.position.y + offset.y, piece.position.z);

				piece.rotation = Quaternion.Lerp(player.rotation, Quaternion.LookRotation(targetPositions[i].position - piece.position), currentPercent);
				piece.position = Vector3.Lerp(initialPosition, targetPos, currentPercent);
			}

			yield return null;
		}

		// switch offsets to positions
		for (int i = 0; i < 10; i++)
			targetOffsets[i] = pieces[i].position;
	}

	IEnumerator SecondPartAnimRoutine(Transform player, Transform[] pieces)
	{
		// move pieces to portal
		while (GetPercent() < secondAnimPercentDuration)
		{
			float secondAnimPercent = (GetPercent() - firstAnimPercentDuration) / (secondAnimPercentDuration - firstAnimPercentDuration);

			float currentPercent = secondPartAnimCurve.Evaluate(secondAnimPercent);

			for (int i = 0; i < 10; i++)
				pieces[i].position = Vector3.Lerp(targetOffsets[i], targetPositions[i].position, currentPercent);

			yield return null;
		}

		// children pieces to portal
		foreach (Transform piece in pieces)
			piece.SetParent(transform);

		// links snake pieces
		for (int i = 0; i < 9; i++)
			pieces[i].GetComponent<SnakePiece>().Init(pieces[i + 1], 0);

		pieces[pieces.Length - 1].GetComponent<SnakePiece>().Init(pieces[0], 0);
	}

	IEnumerator ThirdPartAnimRoutine()
	{
		// animate portal mesh
		while (GetPercent() <= thirdAnimPercentDuration)
		{
			float previousDurations = firstAnimPercentDuration + secondAnimPercentDuration;
			float localPercent = (GetPercent() - previousDurations) / (thirdAnimPercentDuration - previousDurations);

			renderInsidePortal.transform.localScale = Vector3.one * thirdPartAnimCurve.Evaluate(localPercent) * circleRadius;

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