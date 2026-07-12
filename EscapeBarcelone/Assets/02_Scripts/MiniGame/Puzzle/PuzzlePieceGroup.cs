using UnityEngine;
using System.Collections.Generic;

public class PuzzlePieceGroup : MonoBehaviour
{
    private readonly List<PuzzlePiece> pieces = new();

    private readonly HashSet<PuzzlePiece> borderPieces = new();

    public IReadOnlyList<PuzzlePiece> Pieces => pieces;

    public IReadOnlyCollection<PuzzlePiece> BorderPieces => borderPieces;


    public void AddPiece(PuzzlePiece piece)
    {
        if (pieces.Contains(piece))
            return;

        pieces.Add(piece);
        borderPieces.Add(piece);

        piece.Group = this;
        piece.transform.SetParent(transform, true);
    }

    public void Merge(PuzzlePieceGroup other)
    {
        foreach (PuzzlePiece piece in other.pieces)
        {
            AddPiece(piece);
        }

        other.pieces.Clear();
        other.borderPieces.Clear();

        Destroy(other.gameObject);
    }

    public void RefreshBorders(
        Dictionary<Vector2Int, PuzzlePiece> piecesMap,
        Vector2Int[] directions)
    {
        borderPieces.Clear();

        foreach (PuzzlePiece piece in pieces)
        {
            foreach (Vector2Int direction in directions)
            {
                Vector2Int position = piece.GridPosition + direction;

                if (!piecesMap.TryGetValue(position, out PuzzlePiece neighbor)
                    || neighbor.Group != this)
                {
                    borderPieces.Add(piece);
                    break;
                }
            }
        }
    }

    public void Move(Vector3 position)
    {
        transform.position = position;
    }
}