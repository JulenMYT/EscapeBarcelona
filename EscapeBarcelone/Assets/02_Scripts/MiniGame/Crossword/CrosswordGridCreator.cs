using System;
using System.Collections.Generic;
using UnityEngine;

public class CrosswordGridCreator
{
    public Vector2Int LowestPosition { get; private set; }
    public Vector2Int HighestPosition { get; private set; }
    public Vector2Int GridSize => HighestPosition - LowestPosition + Vector2Int.one;
    public Dictionary<Vector2Int, TileData> Tiles { get; private set; }

    public void CreateTiles(CrosswordData data)
    {
        Tiles = new Dictionary<Vector2Int, TileData>();

        foreach (var item in data.words)
        {
            for (int i = 0; i < item.word.Length; i++)
            {
                Vector2Int position = item.originPosition;

                if (item.isDown)
                    position.y += i;
                else
                    position.x += i;

                char correctChar = item.word[i];

                if (Tiles.TryGetValue(position, out TileData existingTile))
                {
                    if (existingTile.CorrectChar != correctChar)
                        Debug.LogError($"Crossword incoherence at {position}: '{existingTile.CorrectChar}' != '{correctChar}'.");

                    if (item.isDown)
                        existingTile.IsVertical = true;
                    else
                        existingTile.IsHorizontal = true;

                    continue;
                }

                Tiles.Add(position, new TileData(position, correctChar, !item.isDown, item.isDown));
            }
        }

        LowestPosition = new Vector2Int(int.MaxValue, int.MaxValue);
        HighestPosition = new Vector2Int(int.MinValue, int.MinValue);

        foreach (var tile in Tiles.Values)
        {
            LowestPosition = Vector2Int.Min(LowestPosition, tile.Position);
            HighestPosition = Vector2Int.Max(HighestPosition, tile.Position);
        }
    }
}

public class TileData
{
    public Vector2Int Position { get; }
    public char CorrectChar { get; }
    public bool IsHorizontal { get; set; }
    public bool IsVertical { get; set; }

    public TileData(Vector2Int position, char correctChar, bool isHorizontal, bool isVertical)
    {
        Position = position;
        CorrectChar = correctChar;
        IsHorizontal = isHorizontal;
        IsVertical = isVertical;
    }
}