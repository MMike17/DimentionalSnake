using UnityEngine;
using static DataManager;

/// <summary>Entry point of the game flow</summary>
public class GameManager : MonoBehaviour
{
	const string SAVE_FILE_NAME = "save";

	[Header("Settings")]
	public LogLevel logLevel;
	public bool isRelease;

	[Header("Managers")]
	public TerrainManager terrainManager;
	public InterfaceManager interfaceManager;
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
				Debug.Log("Game over");
			},
			difficultyManager.GetCurrentSpeed
		);
	}

	void LoadLocalData()
	{
		DataManager.SetLogLevel(logLevel);
		DataManager.SetRelease(isRelease);

		if(DataManager.DoesFileExists(SAVE_FILE_NAME))
			playerData = DataManager.LoadObjectAtPath<PlayerData>(SAVE_FILE_NAME);
		else
			playerData = new PlayerData();
	}

	void InitManagers()
	{
		// TODO : Init managers

		float highscore = playerData.highscore > 0 ? playerData.highscore : scoreManager.initialHighscore;

		terrainManager.Init(
			snake.transform,
			difficultyManager.GetDifficulty,
			difficultyManager.GetCurrentSpeed,
			distance => scoreManager.AddPlayerScore(distance)
		);
		interfaceManager.Init(
			playerData.hasSound,
			() =>
			{
				// TODO : unlock snake here
			},
			state =>
			{
				playerData.hasSound = state;
				// TODO : set sound in game here
			}
		);
		scoreManager.Init(
			playerData.highscore,
			playerData.playerMoney,
			() =>
			{
				// TODO : Trigger new highscore animation
			},
			interfaceManager.UpdateScore,
			interfaceManager.UpdateMoney
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