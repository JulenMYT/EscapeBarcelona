using UnityEngine;
using System.Collections.Generic;
using System;

public class PuzzleConnectionManager
{
    private readonly Dictionary<Vector2Int, PuzzlePiece> pieces = new();

    private const float snapDistance = 0.3f;
    private const float snapDistanceSqr = snapDistance * snapDistance;

    // Écart de profondeur entre deux groupes : assez pour départager l'affichage et le
    // clic (OnMouseDown prend le plus proche de la caméra), assez petit pour rester
    // loin de la caméra même avec beaucoup de groupes.
    private const float DepthStep = 0.01f;

    private static readonly Vector2Int[] directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    public event Action PuzzleCompleted;

    private int groupCount;

    // Du plus en arrière au plus en avant.
    private readonly List<PuzzlePieceGroup> layering = new();

    public void Register(PuzzlePiece piece)
    {
        pieces.Add(piece.GridPosition, piece);

        if (piece.Group != null && !layering.Contains(piece.Group))
        {
            layering.Add(piece.Group);
            ApplyLayering();
        }
    }

    public void SetGroupCount(int count)
    {
        groupCount = count;
    }

    // ───────────────────────── Premier plan ─────────────────────────

    /// <summary>Fait passer ce groupe devant tous les autres (affichage et clic).</summary>
    public void BringToFront(PuzzlePieceGroup group)
    {
        layering.Remove(group);
        layering.Add(group);
        ApplyLayering();
    }

    private void ApplyLayering()
    {
        layering.RemoveAll(g => g == null);

        // Le dernier de la liste est le plus proche de la caméra (z le plus petit).
        int count = layering.Count;

        for (int i = 0; i < count; i++)
            layering[i].SetDepth(-(i + 1) * DepthStep);
    }

    // ───────────────────────── Collage ─────────────────────────

    /// <summary>
    /// Colle le groupe à TOUS ses voisins compatibles, pas seulement au premier :
    /// une pièce posée entre deux voisins se colle aux deux d'un coup.
    /// </summary>
    public void CheckConnection(PuzzlePieceGroup group)
    {
        bool merged = true;

        while (merged && group != null)
        {
            merged = false;

            // Copie : la liste change à chaque fusion.
            foreach (PuzzlePiece piece in new List<PuzzlePiece>(group.BorderPieces))
            {
                if (CheckPieceConnections(piece))
                {
                    merged = true;
                    break;
                }
            }
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

            // Une pièce encore cachée ne peut pas servir de voisin.
            if (!other.gameObject.activeInHierarchy)
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
        Vector2Int difference = b.GridPosition - a.GridPosition;

        Vector3 expectedOffset = new Vector3(difference.x * a.Size.x, difference.y * a.Size.y, 0);
        Vector3 currentOffset = b.transform.position - a.transform.position;

        Vector3 delta = currentOffset - expectedOffset;
        delta.z = 0;   // la profondeur ne compte pas pour l'emboîtement

        return delta.sqrMagnitude < snapDistanceSqr;
    }

    private void Merge(PuzzlePiece a, PuzzlePiece b)
    {
        if (a.Group == b.Group)
            return;

        Vector3 offset = GetSnapOffset(a, b);

        a.Group.transform.position += offset;

        PuzzlePieceGroup absorbed = b.Group;
        a.Group.Merge(absorbed);
        layering.Remove(absorbed);

        UpdateBorders(a.Group);
        BringToFront(a.Group);

        groupCount--;

        if (groupCount == 1)
            PuzzleCompleted?.Invoke();
    }

    private Vector3 GetSnapOffset(PuzzlePiece a, PuzzlePiece b)
    {
        Vector2Int difference = a.GridPosition - b.GridPosition;

        Vector3 expectedPosition = new Vector3(difference.x * a.Size.x, difference.y * a.Size.y, 0);

        Vector3 currentOffset = a.transform.position - b.transform.position;

        Vector3 offset = expectedPosition - currentOffset;
        offset.z = 0;   // on aligne en x/y seulement, la profondeur est gérée par le layering

        return offset;
    }

    private void UpdateBorders(PuzzlePieceGroup group)
    {
        group.RefreshBorders(pieces, directions);
    }

    public void RestoreGroup(PuzzleState.PuzzleGroupState groupState)
    {
        PuzzlePieceGroup targetGroup = null;

        foreach (Vector2Int position in groupState.pieces)
        {
            if (!pieces.TryGetValue(position, out PuzzlePiece piece))
                continue;

            if (targetGroup == null)
            {
                targetGroup = piece.Group;
                continue;
            }

            if (piece.Group == targetGroup)
                continue;

            Merge(targetGroup.Pieces[0], piece);
        }

        if (targetGroup != null)
        {
            Vector3 saved = groupState.position;
            targetGroup.transform.position = new Vector3(saved.x, saved.y, targetGroup.transform.position.z);
            UpdateBorders(targetGroup);
        }
    }
}
