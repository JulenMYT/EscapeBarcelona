using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.U2D.IK;
using UnityEngine.UI;

public class CrosswordFinalAnswer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridLayoutGroup layoutGroup;
    [SerializeField] private AnswerTile letterTile;
    [SerializeField] private InputActionReference backspaceAction;

    private List<LetterTile> answerTiles;
    private string finalWord;
    private int selectedTileIndex;

    public event Action OnAnswerCompleted;

    private void OnEnable()
    {
        backspaceAction.action.performed += OnBackspace;
    }

    private void OnDisable()
    {
        backspaceAction.action.performed -= OnBackspace;
    }

    public void CreateAnswer(CrosswordData data)
    {
        finalWord = data.finalWord;
        answerTiles = new List<LetterTile>();
        selectedTileIndex = 0;

        SetupLayout();
        CreateTiles();

        if (answerTiles.Count > 0)
            SelectTile(0);
    }

    private void SetupLayout()
    {
        int tileCount = finalWord.Length;

        layoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layoutGroup.constraintCount = tileCount;

        float availableWidth = GetLayoutWidth();
        float spacing = layoutGroup.spacing.x * (tileCount - 1);
        float tileSize = (availableWidth - spacing) / tileCount;

        layoutGroup.cellSize = Vector2.one * tileSize;
    }

    private float GetLayoutWidth()
    {
        RectTransform rectTransform = layoutGroup.GetComponent<RectTransform>();
        return rectTransform.rect.width;
    }

    private void CreateTiles()
    {
        for (int i = 0; i < finalWord.Length; i++)
        {
            AnswerTile tile = Instantiate(letterTile, layoutGroup.transform);

            tile.OnLetterEntered += OnLetterEntered;
            tile.OnTileClicked += OnTileClicked;

            tile.Setup(i + 1);
            answerTiles.Add(tile);
        }
    }

    private void OnLetterEntered(LetterTile tile)
    {
        if (CheckAnswer())
        {
            LockAnswer();
            OnAnswerCompleted?.Invoke();
            return;
        }

        int index = answerTiles.IndexOf(tile);

        if (index < answerTiles.Count - 1)
            SelectTile(index + 1);
    }

    private void OnTileClicked(LetterTile tile)
    {
        answerTiles[selectedTileIndex].SetBase();
        tile.SetSelected();
    }

    private void SelectTile(int index)
    {
        if (selectedTileIndex < answerTiles.Count)
            answerTiles[selectedTileIndex].SetBase();

        selectedTileIndex = index;

        LetterTile tile = answerTiles[selectedTileIndex];
        tile.SetSelected();
        tile.Focus();
    }

    private void OnBackspace(InputAction.CallbackContext context)
    {
        if (answerTiles[selectedTileIndex].HasLetter())
        {
            answerTiles[selectedTileIndex].ClearLetter();
            return;
        }

        SelectTile(selectedTileIndex - 1);
    }

    private bool CheckAnswer()
    {
        for (int i = 0; i < answerTiles.Count; i++)
        {
            if (!answerTiles[i].HasLetter())
                return false;

            if (answerTiles[i].GetEnteredLetter() != char.ToUpper(finalWord[i]))
                return false;
        }

        return true;
    }

    private void LockAnswer()
    {
        foreach (LetterTile tile in answerTiles)
        {
            tile.Lock();
            tile.SetCorrect();
        }
    }

    public string GetAnswer()
    {
        string answer = string.Empty;

        if (answerTiles == null)
            return answer;

        foreach (LetterTile tile in answerTiles)
        {
            if (tile.HasLetter())
                answer += tile.GetEnteredLetter();
            else
                answer += ' ';
        }

        return answer;
    }

    public void RestoreState(CrosswordState state)
    {
        for (int i = 0; i < answerTiles.Count && i < state.finalAnswer.Length; i++)
            answerTiles[i].SetLetter(state.finalAnswer[i]);
    }

    public void Solve()
    {
        for (int i = 0; i < answerTiles.Count; i++)
        {
            answerTiles[i].SetLetter(finalWord[i]);
            answerTiles[i].Lock();
            answerTiles[i].SetCorrect();
        }
    }
}