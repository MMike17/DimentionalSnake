using UnityEngine;

/// <summary>Settings for snake pieces</summary>
[CreateAssetMenu(fileName = "SnakePieces", menuName = "DimentionalSnake/SnakePieces")]
public class SnakePieces : ScriptableObject
{
	public string settingsName;
	public GameObject piecePrefab;
	public Sprite shopSnapshot;
}