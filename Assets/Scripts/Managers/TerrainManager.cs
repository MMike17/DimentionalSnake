using UnityEngine;

/// <summary>Manages the terrain generation, obstacles and bonuses</summary>
public class TerrainManager : BaseBehaviour
{
	[Header("Settings")]
	public float deleteDistance;

	[Header("Scene references")]
	public TerrainGenerator terrainGenerator;

	// TODO : Pieces generation
	// TODO : Money generation
	// TODO : Obstacles generation

	public void Init(Transform player)
	{
		terrainGenerator.Init(deleteDistance, chunk =>
		{
			Vector3 playerPos = player.position;
			playerPos.x = 0;
			playerPos.y = 0;

			return Vector3.Distance(playerPos, chunk.position);
		});

		InitInternal();
	}
}