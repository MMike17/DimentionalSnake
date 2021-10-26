using System;
using TMPro;
using UnityEngine;

/// <summary>Manages what is displayed on the interface during the game phase</summary>
public class GameInterface : BaseInterface
{
	[Header("Settings")]
	public float horizontalUIOffset;
	public float verticalUIOffset, uiMovementSpeed;

	[Header("Scene references")]
	public TextMeshProUGUI scoreDisplay;
	public TextMeshProUGUI piecesCounter;

	Func<Vector3> GetPlayerScreenPosition;

	public void Init(Func<Vector3> getPlayerScreenPosition)
	{
		GetPlayerScreenPosition = getPlayerScreenPosition;

		InitInternal();
	}

	void Update()
	{
		if(!initialized)
			return;

		Vector3 playerUIPos = GetPlayerScreenPosition();
		Vector3 offset = new Vector3(0, verticalUIOffset, 0);

		offset.x = (playerUIPos.x >= Screen.width / 2 ? -1 : 1) * horizontalUIOffset;

		piecesCounter.transform.position = Vector3.MoveTowards(piecesCounter.transform.position, playerUIPos + offset, uiMovementSpeed * Time.deltaTime);
	}

	public void UpdateScore(float value)
	{
		if(!CheckInitialized())
			return;

		scoreDisplay.text = Mathf.RoundToInt(value).ToString();
	}

	public void UpdatePiecesCount(int count)
	{
		if(!CheckInitialized())
			return;

		piecesCounter.text = count.ToString();
	}
}