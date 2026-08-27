using System;
using TMPro;
using UnityEngine;

public class CrosswordLetterTile : LetterTile
{
    [SerializeField] private Color hintColor;
    [SerializeField] private TMP_Text numberText;
    [SerializeField] private TMP_Text numberText2;

    public TileData TileData { get; private set; }

    public void Setup(TileData tileData)
    {
        TileData = tileData;

        numberText.text = tileData.Number?.ToString() ?? string.Empty;
        numberText2.text = string.Empty;

        ClearLetter();
        SetBase();
    }

    public void ShowHint()
    {
        numberText2.text = TileData.FinalWordNumber.ToString();
        numberText.text = string.Empty;
        if (!String.IsNullOrEmpty(numberText2.text))
        {
            inputField.image.color = hintColor;
        }
        else
        {
            inputField.image.color = baseColor;
        }
    }
}