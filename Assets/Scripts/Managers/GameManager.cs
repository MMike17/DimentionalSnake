using UnityEngine;

/// <summary>Entry point of the game flow</summary>
public class GameManager : MonoBehaviour
{
	[Header("Managers")]
	public TerrainManager terrainManager;

	[Header("Uniques")]
	public Snake snake;

	void Awake()
	{
		InitManagers();

		snake.Init();
	}

	void InitManagers()
	{
		// TODO : Init managers
		terrainManager.Init();
	}
}