using TMPro;
using UnityEngine;

public class CrosswordLetterTile : LetterTile
{
    [SerializeField] private TMP_Text numberText;

    public TileData TileData { get; private set; }

    public void Setup(TileData tileData)
    {
        TileData = tileData;

        numberText.text = tileData.Number?.ToString() ?? string.Empty;

        ClearLetter();
        SetBase();
    }
}