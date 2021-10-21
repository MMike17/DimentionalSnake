using UnityEngine;

/// <summary>Entry point of the game flow</summary>
public class GameManager : MonoBehaviour
{
	[Header("Managers")]
	public TerrainManager terrainManager;
	public ScoreManager scoreManager;

	[Header("Uniques")]
	public Snake snake;
	public PlayerInput playerInput;

	void Awake()
	{
		InitManagers();
		SubscribePlayerInput();

		snake.Init();
	}

	void InitManagers()
	{
		// TODO : Init managers

		terrainManager.Init(snake.transform);
		scoreManager.Init();
	}

	void SubscribePlayerInput()
	{
		playerInput.SubscribeHorizontalInputEvent(snake.SetXPosOnTerrain);
	}
}