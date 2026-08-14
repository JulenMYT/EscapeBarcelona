using System.Collections.Generic;
using UnityEngine;

public class PuzzleHidePiece
{
    public List<PuzzlePieceGroup> hiddenPieces { get; private set; } = new();

    public void HidePieces(List<PuzzlePieceGroup> pieces, int piecesToHide)
    {
        hiddenPieces.Clear();

        ShuffleList(pieces);

        piecesToHide = Mathf.Min(piecesToHide, pieces.Count);

        for (int i = 0; i < piecesToHide; i++)
        {
            PuzzlePieceGroup pieceGroup = pieces[i];
            hiddenPieces.Add(pieceGroup);
            pieceGroup.gameObject.SetActive(false);
        }
    }

    public void RevealHiddenPiece(int number = 1)
    {
        number = Mathf.Min(number, hiddenPieces.Count);

        for (int i = 0; i < number; i++)
        {
            hiddenPieces[0].gameObject.SetActive(true);
            hiddenPieces.RemoveAt(0);
        }
    }

    private void ShuffleList(List<PuzzlePieceGroup> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);

            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }
}