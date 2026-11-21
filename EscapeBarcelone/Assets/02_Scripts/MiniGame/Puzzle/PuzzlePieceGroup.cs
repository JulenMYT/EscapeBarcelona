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

        // Les pièces sont toutes à z = 0 dans le groupe : c'est le z du GROUPE qui
        // décide de celui qui passe devant (voir PuzzleConnectionManager.BringToFront).
        Vector3 local = piece.transform.localPosition;
        piece.transform.localPosition = new Vector3(local.x, local.y, 0f);
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

    public void SetDepth(float z)
    {
        Vector3 position = transform.position;
        transform.position = new Vector3(position.x, position.y, z);
    }

    /// <summary>
    /// Limite une position voulue pour que tout le groupe reste dans la zone.
    /// Comme avant pour une pièce seule : le centre de chaque pièce du bord reste dans
    /// la zone (une demi-pièce peut dépasser). Un grand groupe ne peut donc plus sortir
    /// de l'écran en n'étant retenu que par son point d'attache.
    /// </summary>
    public Vector3 ClampToArea(Vector3 desiredPosition, Bounds area)
    {
        if (Pieces.Count == 0)
            return desiredPosition;

        // Étendue des centres des pièces, par rapport à la position du groupe.
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, 0f);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, 0f);

        foreach (PuzzlePiece piece in Pieces)
        {
            Vector3 offset = piece.transform.position - transform.position;
            min = Vector3.Min(min, offset);
            max = Vector3.Max(max, offset);
        }

        return new Vector3(
            ClampAxis(desiredPosition.x, area.min.x - min.x, area.max.x - max.x),
            ClampAxis(desiredPosition.y, area.min.y - min.y, area.max.y - max.y),
            desiredPosition.z);
    }

    private static float ClampAxis(float value, float low, float high)
    {
        // Groupe plus grand que la zone : on le centre au mieux.
        if (low > high)
            return (low + high) * 0.5f;

        return Mathf.Clamp(value, low, high);
    }
}
