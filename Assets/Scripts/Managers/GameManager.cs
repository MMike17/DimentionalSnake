using UnityEngine;
using static DataManager;
using static ShopInterface;

/// <summary>Entry point of the game flow</summary>
public class GameManager : MonoBehaviour
{
	// TODO : Optimize scripts which are too big

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
	public BackgroundManager backgroundManager;

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
			interfaceManager.shopInterface.snakeSettings[playerData.selectedSettings].piecePrefab,
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
				backgroundManager.PickNewBackground();

				bonusTerrainManager.SpawnTerrain(
					CameraManager.PORTAL_RENDER_LAYER,
					difficultyManager.GetDifficulty(),
					difficultyManager.GetSpeedFromDifficulty(difficultyManager.GetDifficulty()),
					() => { return terrainManager.GetLastChunkPosition(); },
					snake
				);

				terrainManager.StartBonus();
				interfaceManager.UpdatePiecesCount(0);
			},
			difficultyManager.ForceSpeedDown,
			portal =>
			{
				terrainManager.StopBonus();

				bonusTerrainManager.StopBonus();

				portal.SwitchWorlds();
				cameraManager.SwitchCamera();
			},
			interfaceManager.UpdatePiecesCount,
			portal =>
			{
				terrainManager.HideTerrain();

				bonusTerrainManager.StartBonus();

				portal.SwitchWorlds();
				cameraManager.SwitchCamera();
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
				snake.Reset();
				difficultyManager.Reset();

				snake.Unfreeze();
				terrainManager.Unfreeze();
			},
			() =>
			{
				cameraManager.Reset();

				snake.Reset();
				terrainManager.Reset();
				bonusTerrainManager.Reset();

				scoreManager.Reset();
			},
			amount => scoreManager.GiveMoney(amount),
			amount =>
			{
				ShopPlayerSetting settings = interfaceManager.shopInterface.GetShopPlayerSetting();

				scoreManager.TakeMoney(amount);
				snake.SetSnakePrefab(interfaceManager.shopInterface.snakeSettings[settings.selectedIndex].piecePrefab);
			},
			index => snake.SetSnakePrefab(interfaceManager.shopInterface.snakeSettings[index].piecePrefab),
			state =>
			{
				playerData.hasSound = state;
				// TODO : set sound in game here
			},
			fakeAdManager.PopAd,
			() =>
			{
				return scoreManager.CanBuy(200);
			},
			() => { return cameraManager.mainCamera.WorldToScreenPoint(snake.transform.position); }
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
		bonusTerrainManager.Init(
			CameraManager.BONUS_RENDER_LAYER,
			CameraManager.PORTAL_RENDER_LAYER,
			cameraManager.SetRendererToCamera,
			terrainManager.GetCanMove,
			difficultyManager.GetCurrentSpeed,
			position => { return position.z < cameraManager.mainCamera.transform.position.z; },
			position => { return 1 - (position.z / terrainManager.spawnPoint.position.z); }
		);
		backgroundManager.Init();
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