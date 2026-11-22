using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Le mot final (SAGRADA FAMILIA), qui apparaît quand la grille est terminée : une case
/// par lettre, numérotée comme les cases roses de la grille.
/// </summary>
public class CrosswordFinalAnswer : MonoBehaviour
{
    public event Action OnAnswerCompleted;

    [Header("References")]
    [SerializeField] private GridLayoutGroup layoutGroup;
    [SerializeField] private AnswerTile letterTile;
    [SerializeField] private InputActionReference backspaceAction;

    private List<LetterTile> answerTiles;

    // Le mot tel qu'écrit dans les données (« SAGRADA FAMILIA ») : un espace laisse une case vide.
    private string displayedWord;

    // Seulement les lettres (« SAGRADAFAMILIA ») : une case de réponse par lettre.
    private string finalWord;
    private int selectedTileIndex;

    private void OnEnable()
    {
        backspaceAction.action.performed += OnBackspace;
    }

    private void OnDisable()
    {
        backspaceAction.action.performed -= OnBackspace;
    }

    // ───────────────────────── Création ─────────────────────────

    public void CreateAnswer(CrosswordData data)
    {
        displayedWord = (data.finalWord ?? string.Empty).Trim();
        finalWord = new string(displayedWord.Where(char.IsLetter).ToArray());
        answerTiles = new List<LetterTile>();
        selectedTileIndex = 0;

        SetupLayout();
        CreateTiles();

        if (answerTiles.Count > 0)
            SelectTile(0);
    }

    private void SetupLayout()
    {
        int tileCount = displayedWord.Length;

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
        foreach (char c in displayedWord)
        {
            // Espace entre deux mots : une case vide, sans rien dedans.
            if (!char.IsLetter(c))
            {
                new GameObject("Space", typeof(RectTransform)).transform.SetParent(layoutGroup.transform, false);
                continue;
            }

            AnswerTile tile = Instantiate(letterTile, layoutGroup.transform);

            tile.OnLetterEntered += OnLetterEntered;
            tile.OnTileClicked += OnTileClicked;

            // Numéro = position de la lettre (les espaces ne comptent pas), comme dans la grille.
            tile.Setup(answerTiles.Count + 1);
            answerTiles.Add(tile);
        }
    }

    // ───────────────────────── Sélection et saisie ─────────────────────────

    private void OnTileClicked(LetterTile tile)
    {
        int index = answerTiles.IndexOf(tile);

        if (index < 0 || tile.IsLocked)
            return;

        SelectTile(index);
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

    private void OnBackspace(InputAction.CallbackContext context)
    {
        if (CutsceneDirector.IsPlaying)
            return;

        // La réponse finale n'existe qu'une fois la grille terminée.
        if (answerTiles == null || answerTiles.Count == 0)
            return;

        LetterTile selectedTile = answerTiles[selectedTileIndex];

        if (selectedTile.IsLocked)
            return;

        if (selectedTile.HasLetter())
        {
            selectedTile.ClearLetter();
            return;
        }

        if (selectedTileIndex > 0)
            SelectTile(selectedTileIndex - 1);
    }

    // ───────────────────────── Vérification ─────────────────────────

    private bool CheckAnswer()
    {
        for (int i = 0; i < answerTiles.Count; i++)
        {
            if (!answerTiles[i].HasLetter())
                return false;

            if (!CrosswordText.SameLetter(answerTiles[i].GetEnteredLetter(), finalWord[i]))
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

    public void Solve()
    {
        for (int i = 0; i < answerTiles.Count; i++)
        {
            answerTiles[i].SetLetter(finalWord[i]);
            answerTiles[i].Lock();
            answerTiles[i].SetCorrect();
        }
    }

    // ───────────────────────── Sauvegarde ─────────────────────────

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
        // Les cases vides sont sauvegardées comme des espaces : on ne les remplit pas,
        // sinon elles compteraient comme « remplies » (avec un espace invisible).
        for (int i = 0; i < answerTiles.Count && i < state.finalAnswer.Length; i++)
        {
            if (char.IsLetter(state.finalAnswer[i]))
                answerTiles[i].SetLetter(state.finalAnswer[i]);
        }
    }
}