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

        CreateTileData(data);
        AssignWordNumbers(data);
        CalculateBounds();
        SetFinalWordLetters(data);
    }

    private void CreateTileData(CrosswordData data)
    {
        foreach (WordData word in data.words)
        {
            for (int i = 0; i < word.word.Length; i++)
            {
                Vector2Int position = word.originPosition;

                if (word.isDown)
                    position.y += i;
                else
                    position.x += i;

                char correctChar = word.word[i];

                if (Tiles.TryGetValue(position, out TileData existingTile))
                {
                    if (existingTile.CorrectChar != correctChar)
                        Debug.LogError($"Crossword incoherence at {position}: '{existingTile.CorrectChar}' != '{correctChar}'.");

                    if (word.isDown)
                        existingTile.IsVertical = true;
                    else
                        existingTile.IsHorizontal = true;

                    continue;
                }

                Tiles.Add(position, new TileData(
                    position,
                    correctChar,
                    !word.isDown,
                    word.isDown
                ));
            }
        }
    }

    private void AssignWordNumbers(CrosswordData data)
    {
        List<WordData> sortedWords = new(data.words);

        sortedWords.Sort((a, b) =>
        {
            int yComparison = a.originPosition.y.CompareTo(b.originPosition.y);

            if (yComparison != 0)
                return yComparison;

            return a.originPosition.x.CompareTo(b.originPosition.x);
        });

        int currentNumber = 1;

        foreach (WordData word in sortedWords)
        {
            TileData tile = Tiles[word.originPosition];

            if (!tile.Number.HasValue)
                tile.Number = currentNumber++;

            word.Number = tile.Number.Value;
        }
    }

    private void CalculateBounds()
    {
        LowestPosition = new Vector2Int(int.MaxValue, int.MaxValue);
        HighestPosition = new Vector2Int(int.MinValue, int.MinValue);

        foreach (TileData tile in Tiles.Values)
        {
            LowestPosition = Vector2Int.Min(LowestPosition, tile.Position);
            HighestPosition = Vector2Int.Max(HighestPosition, tile.Position);
        }
    }

    private void SetFinalWordLetters(CrosswordData data)
    {
        for (int i = 0; i < data.finalWordLetterPositions.Count; i++)
        {
            Vector2Int position = data.finalWordLetterPositions[i];

            if (!Tiles.TryGetValue(position, out TileData tile))
            {
                Debug.LogError($"Final word position {position} does not contain a tile.");
                continue;
            }

            tile.FinalWordNumber = i + 1;
        }
    }
}

public class TileData
{
    public Vector2Int Position { get; }
    public char CorrectChar { get; }
    public int? Number { get; set; }
    public int? FinalWordNumber { get; set; }
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