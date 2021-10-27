using UnityEngine;

/// <summary>Used to make snapshots</summary>
public class TestLink : MonoBehaviour
{
	[Header("Scene references")]
	public SnakePiece head;
	public SnakePiece[] piecesRed, piecesBlue, piecesDark, piecesGreen, piecesLight, piecesNeon, piecesPurple, piecesSwamp, piecesUgly;

	void Awake()
	{
		head.Init(piecesRed[0].transform, 0);

		SetPieces(piecesRed);
		SetPieces(piecesBlue);
		SetPieces(piecesDark);
		SetPieces(piecesGreen);
		SetPieces(piecesLight);
		SetPieces(piecesNeon);
		SetPieces(piecesPurple);
		SetPieces(piecesSwamp);
		SetPieces(piecesUgly);
	}

	void SetPieces(SnakePiece[] pieces)
	{
		if(pieces[0].gameObject.activeSelf)
		{
			for (int i = 0; i < pieces.Length - 1; i++)
				pieces[i].Init(pieces[i + 1].transform, 0);

			pieces[pieces.Length - 1].Init(null, 0);
		}
	}
}