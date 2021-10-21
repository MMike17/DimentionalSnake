using UnityEngine;

/// <summary>Entry point of the game flow</summary>
public class GameManager : MonoBehaviour
{
	const string SAVE_FILE_NAME = "save";

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
		LoadLocalData();
		InitManagers();
		SubscribePlayerInput();

		snake.Init(
			terrainManager.minX.position.x,
			terrainManager.maxX.position.x,
			scoreManager.GetMoney,
			() =>
			{
				// TODO : Implemente game over
			}
		);
	}

	void LoadLocalData()
	{
		if(DataManager.DoesFileExists(SAVE_FILE_NAME))
			playerData = DataManager.LoadObjectAtPath<PlayerData>(SAVE_FILE_NAME);
		else
			playerData = new PlayerData();
	}

	void InitManagers()
	{
		// TODO : Init managers

		float highscore = playerData.highscore > 0 ? playerData.highscore : initialHighscore;

		terrainManager.Init(
			snake.transform,
			difficultyManager.GetDifficulty,
			difficultyManager.GetCurrentSpeed,
			distance => scoreManager.AddPlayerScore(distance)
		);
		scoreManager.Init(
			playerData.highscore,
			() =>
			{
				// TODO : Trigger new highscore animation
			}
		);
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