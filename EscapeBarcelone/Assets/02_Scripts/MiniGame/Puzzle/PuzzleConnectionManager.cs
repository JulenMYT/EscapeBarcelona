using UnityEngine;
using System.Collections.Generic;

public class PuzzleConnectionManager
{
    private readonly Dictionary<Vector2Int, PuzzlePiece> pieces = new();

    private const float snapDistance = 0.3f;

    private static readonly Vector2Int[] directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    public void Register(PuzzlePiece piece)
    {
        pieces.Add(piece.GridPosition, piece);
    }

    public void CheckConnection(PuzzlePieceGroup group)
    {
        foreach (PuzzlePiece piece in group.Pieces)
        {
            if (CheckPieceConnections(piece))
                break;
        }
    }

    private bool CheckPieceConnections(PuzzlePiece piece)
    {
        foreach (Vector2Int direction in directions)
        {
            Vector2Int neighborPosition = piece.GridPosition + direction;

            if (!pieces.TryGetValue(neighborPosition, out PuzzlePiece other))
                continue;

            if (other.Group == piece.Group)
                continue;

            if (CanConnect(piece, other))
            {
                Merge(piece, other);
                return true;
            }
        }

        return false;
    }

    private bool CanConnect(PuzzlePiece a, PuzzlePiece b)
    {
        Vector3 aLocalPosition =
            a.transform.position;

        Vector3 bLocalPosition =
            b.transform.position;

        Vector2Int difference = b.GridPosition - a.GridPosition;

        Vector3 expectedOffset = new Vector3(
            difference.x * a.Size.x,
            difference.y * a.Size.y,
            0
        );

        Vector3 currentOffset = bLocalPosition - aLocalPosition;

        float distance = Vector3.Distance(
            currentOffset,
            expectedOffset
        );

        return distance < snapDistance;
    }

    private void Merge(PuzzlePiece a, PuzzlePiece b)
    {
        if (a.Group == b.Group)
            return;

        Vector3 offset = GetSnapOffset(a, b);

        a.Group.transform.position += offset;

        a.Group.Merge(b.Group);
    }

    private Vector3 GetSnapOffset(PuzzlePiece a, PuzzlePiece b)
    {
        Vector2Int difference = a.GridPosition - b.GridPosition;

        Vector3 expectedPosition = new Vector3(
            difference.x * a.Size.x,
            difference.y * a.Size.y,
            0
        );

        Vector3 currentOffset =
            a.transform.position - b.transform.position;

        return expectedPosition - currentOffset;
    }
}