using UnityEngine;
using static DataManager;
using static ShopInterface;

/// <summary>Entry point of the game flow</summary>
public class GameManager : MonoBehaviour
{
	const string SAVE_FILE_NAME = "save.bin";

	[Header("Settings")]
	public LogLevel logLevel;
	public bool isRelease;

	[Header("Managers")]
	public TerrainManager terrainManager;
	public InterfaceManager interfaceManager;
	public ScoreManager scoreManager;
	public DifficultyManager difficultyManager;
	public FakeAdManager fakeAdManager;
	public CameraManager cameraManager;
	public BonusTerrainManager bonusTerrainManager;

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
			() => scoreManager.GiveMoney(),
			() =>
			{
				cameraManager.StartLoseAnimation(() => interfaceManager.GameOver(scoreManager.GetCurrentScore(), scoreManager.GetHighscore()));

				snake.Freeze();
				terrainManager.Freeze();
			},
			() =>
			{
				interfaceManager.GameOver(scoreManager.GetCurrentScore(), scoreManager.GetHighscore());

				snake.Freeze();
				terrainManager.Freeze();
			},
			() =>
			{
				Vector3 target = snake.transform.position;
				target.y = terrainManager.GetTargetHeight();

				snake.transform.position = target;
				terrainManager.PositionCamera();
			},
			difficultyManager.GetCurrentSpeed
		);
	}

	void Update()
	{
		DelayedActionsManager.Update(Time.deltaTime);
	}

	void LoadLocalData()
	{
		DataManager.SetLogLevel(logLevel);
		DataManager.SetRelease(isRelease);

		if(DataManager.DoesFileExists(SAVE_FILE_NAME))
			playerData = DataManager.LoadObjectAtPath<PlayerData>(SAVE_FILE_NAME);
		else
			playerData = new PlayerData(interfaceManager.GetUnlocksCount());
	}

	void InitManagers()
	{
		// TODO : Init managers

		float highscore = playerData.highscore > 0 ? playerData.highscore : scoreManager.initialHighscore;

		difficultyManager.Init(
			highscore,
			scoreManager.GetCurrentScore
		);
		terrainManager.Init(
			snake.transform,
			cameraManager.mainCamera,
			difficultyManager.GetDifficulty,
			difficultyManager.GetCurrentSpeed,
			distance => scoreManager.AddPlayerScore(distance)
		);
		interfaceManager.Init(
			playerData.hasSound,
			playerData.unlockedSettings,
			playerData.selectedSettings,
			() =>
			{
				snake.Unfreeze();
				terrainManager.Unfreeze();
			},
			() =>
			{
				cameraManager.Reset();

				snake.Reset();
				terrainManager.Reset();
			},
			amount => scoreManager.GiveMoney(amount),
			amount => scoreManager.TakeMoney(amount),
			state =>
			{
				playerData.hasSound = state;
				// TODO : set sound in game here
			},
			fakeAdManager.PopAd,
			() =>
			{
				return scoreManager.CanBuy(200);
			}
		);
		scoreManager.Init(
			playerData.highscore,
			Mathf.Abs(terrainManager.spawnPoint.position.z - snake.transform.position.z),
			playerData.playerMoney,
			terrainManager.SpawnNewHighscore,
			interfaceManager.UpdateScore,
			interfaceManager.UpdateMoney
		);
		fakeAdManager.Init();
		cameraManager.Init(snake.transform);
		bonusTerrainManager.Init(distance => scoreManager.AddPlayerScore(distance));
	}

	void SubscribePlayerInput()
	{
		playerInput.SubscribeHorizontalInputEvent(snake.SetXPosOnTerrain);
	}

	void OnApplicationFocus(bool state)
	{
		if(!state)
			SaveData();
	}

	void OnApplicationQuit()
	{
		SaveData();
	}

	void SaveData()
	{
		ShopPlayerSetting settings = interfaceManager.shopInterface.GetShopPlayerSetting();
		playerData.selectedSettings = settings.selectedIndex;
		playerData.unlockedSettings = settings.unlocks;

		playerData.highscore = scoreManager.GetHighscore();
		playerData.playerMoney = scoreManager.GetMoney();

		DataManager.SaveObject(playerData, SAVE_FILE_NAME);
	}
}