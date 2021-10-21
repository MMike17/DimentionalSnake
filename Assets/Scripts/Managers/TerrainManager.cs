using UnityEngine;

/// <summary>Manages the terrain generation, obstacles and bonuses</summary>
public class TerrainManager : BaseBehaviour
{
	[Header("Settings")]
	public float deleteDistance;

	[Header("Scene references")]
	public TerrainGenerator terrainGenerator;
	[Space]
	public Transform spawnPoint;

	// TODO : Pieces generation
	// TODO : Money generation
	// TODO : Obstacles generation

	public void Init(Transform player)
	{
		terrainGenerator.Init(
			deleteDistance,
			() => { return new Vector3(0, 0, spawnPoint.position.z); },
			chunk =>
			{
				Vector3 playerPos = player.position;
				playerPos.x = 0;
				playerPos.y = 0;

				return Vector3.Distance(playerPos, chunk.position);
			});

		InitInternal();
	}
}