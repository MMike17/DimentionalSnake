using UnityEngine;

/// <summary>Entry point of the game flow</summary>
public class GameManager : MonoBehaviour
{
	[Header("Settings")]
	public float initialHighscore;

	[Header("Managers")]
	public TerrainManager terrainManager;
	public ScoreManager scoreManager;
	public DifficultyManager difficultyManager;

	[Header("Uniques")]
	public Snake snake;
	public PlayerInput playerInput;

	PlayerData playerData;

	void Awake()
	{
		InitManagers();
		SubscribePlayerInput();

		snake.Init();
	}

	void InitManagers()
	{
		// TODO : Init managers

		float highscore = playerData.highscore > 0 ? playerData.highscore : initialHighscore;

		terrainManager.Init(snake.transform, difficultyManager.GetDifficulty);
		scoreManager.Init();
		difficultyManager.Init(
			highscore,
			scoreManager.GetCurrentScore
		);
	}

	void SubscribePlayerInput()
	{
		playerInput.SubscribeHorizontalInputEvent(snake.SetXPosOnTerrain);
	}
}