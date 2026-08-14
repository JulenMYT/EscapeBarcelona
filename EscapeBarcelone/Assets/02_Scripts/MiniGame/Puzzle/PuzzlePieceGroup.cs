using UnityEngine;
using System.Collections.Generic;

public class PuzzlePieceGroup : MonoBehaviour
{
    public List<PuzzlePiece> Pieces { get; private set; } = new();
    public HashSet<PuzzlePiece> BorderPieces { get; private set; } = new();

    public void AddPiece(PuzzlePiece piece)
    {
        if (Pieces.Contains(piece))
            return;

        if (Pieces.Count == 0)
            BorderPieces.Add(piece);

        Pieces.Add(piece);

        piece.Group = this;
        piece.transform.SetParent(transform, true);
    }

    public void Merge(PuzzlePieceGroup other)
    {
        foreach (PuzzlePiece piece in other.Pieces)
        {
            AddPiece(piece);
        }

        Destroy(other.gameObject);
    }

    public void RefreshBorders(Dictionary<Vector2Int, PuzzlePiece> piecesMap, Vector2Int[] directions)
    {
        BorderPieces.Clear();

        foreach (PuzzlePiece piece in Pieces)
        {
            foreach (Vector2Int direction in directions)
            {
                Vector2Int position = piece.GridPosition + direction;

                if (!piecesMap.TryGetValue(position, out PuzzlePiece neighbor))
                    continue;

                if (neighbor.Group != this)
                {
                    BorderPieces.Add(piece);
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