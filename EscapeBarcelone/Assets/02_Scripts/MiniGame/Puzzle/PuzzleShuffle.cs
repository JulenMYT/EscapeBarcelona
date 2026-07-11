using UnityEngine;
using System.Collections.Generic;

public class PuzzleShuffleManager
{
    private readonly Bounds shuffleArea;
    private readonly Bounds centerHole;

    public PuzzleShuffleManager(
        Bounds shuffleArea,
        Bounds centerHole)
    {
        this.shuffleArea = shuffleArea;
        this.centerHole = centerHole;
    }

    public void Shuffle(IReadOnlyList<PuzzlePieceGroup> groups)
    {
        List<Vector3> positions = GenerateBorderPositions(groups.Count);

        ShuffleList(positions);

        for (int i = 0; i < groups.Count; i++)
        {
            groups[i].transform.position = positions[i];
        }
    }

    private List<Vector3> GenerateBorderPositions(int count)
    {
        List<Vector3> positions = new();


        float perimeter =
            shuffleArea.size.x * 2 +
            shuffleArea.size.y * 2;

        float spacing = perimeter / count;

        for (int i = 0; i < count; i++)
        {
            positions.Add(
                GetRingPosition(
                    shuffleArea,
                    centerHole,
                    i * spacing
                )
            );
        }

        return positions;
    }

    private Vector3 GetRingPosition(
        Bounds outer,
        Bounds inner,
        float distance)
    {
        Vector2 bandSize = GetBandSize(outer, inner);

        float width = outer.size.x;
        float height = outer.size.y;

        if (distance < width)
        {
            return new Vector3(
                outer.min.x + distance,
                outer.max.y - Random.Range(0, bandSize.y),
                0
            );
        }

        distance -= width;

        if (distance < height)
        {
            return new Vector3(
                outer.max.x - Random.Range(0, bandSize.x),
                outer.min.y + distance,
                0
            );
        }

        distance -= height;

        if (distance < width)
        {
            return new Vector3(
                outer.max.x - distance,
                outer.min.y + Random.Range(0, bandSize.y),
                0
            );
        }

        distance -= width;

        return new Vector3(
            outer.min.x + Random.Range(0, bandSize.x),
            outer.max.y - distance,
            0
        );
    }

    private Vector2 GetBandSize(Bounds outer, Bounds inner)
    {
        return new Vector2(
            (outer.size.x - inner.size.x) / 2,
            (outer.size.y - inner.size.y) / 2
        );
    }

    private void ShuffleList(List<Vector3> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);

            Vector3 temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}