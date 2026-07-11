using UnityEngine;
using System.Collections.Generic;

public class PuzzlePieceGroup : MonoBehaviour
{
    private readonly List<PuzzlePiece> pieces = new();

    public IReadOnlyList<PuzzlePiece> Pieces => pieces;

    public void AddPiece(PuzzlePiece piece)
    {
        if (pieces.Contains(piece))
            return;

        pieces.Add(piece);
        piece.Group = this;
        piece.transform.SetParent(transform, true);
    }

    public void Merge(PuzzlePieceGroup other)
    {
        foreach (PuzzlePiece piece in other.pieces.ToArray())
        {
            AddPiece(piece);
        }

        other.pieces.Clear();
        Destroy(other.gameObject);
    }

    public void Move(Vector3 position)
    {
        transform.position = position;
    }
}