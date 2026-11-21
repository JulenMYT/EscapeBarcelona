using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CrosswordView : MonoBehaviour
{
    public event Action OnCrosswordCompleted;

    [Header("References")]
    [SerializeField] private GridLayoutGroup layoutGroup;
    [SerializeField] private CrosswordLetterTile letterTilePrefab;
    [SerializeField] private GameObject emptyTilePrefab;
    [SerializeField] private RectTransform grid;
    [SerializeField] private InputActionReference backspaceAction;

    [Header("Grid Settings")]
    [SerializeField] private float tileSpacing = 5;

    private Dictionary<Vector2Int, CrosswordLetterTile> tiles;
    private HashSet<CrosswordLetterTile> highlightedTiles;
    private CrosswordLetterTile selectedTile;

    private bool currentSelectionIsDown = true;

    public bool solved { get; private set; }

    private void OnEnable()
    {
        backspaceAction.action.performed += OnBackspace;
    }

    private void OnDisable()
    {
        backspaceAction.action.performed -= OnBackspace;
    }

    public void GenerateGrid(CrosswordGridCreator gridCreator)
    {
        InitializeGrid();
        ConfigureGridLayout(gridCreator);
        CreateTiles(gridCreator);
    }

    private void InitializeGrid()
    {
        tiles = new Dictionary<Vector2Int, CrosswordLetterTile>();
        highlightedTiles = new HashSet<CrosswordLetterTile>();
        selectedTile = null;
        solved = false;
    }

    private void ConfigureGridLayout(CrosswordGridCreator gridCreator)
    {
        float availableWidth = grid.rect.width - (gridCreator.GridSize.x - 1) * tileSpacing;
        float availableHeight = grid.rect.height - (gridCreator.GridSize.y - 1) * tileSpacing;

        float calculatedTileSize = Mathf.Min(availableWidth / gridCreator.GridSize.x, availableHeight / gridCreator.GridSize.y);

        layoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layoutGroup.constraintCount = gridCreator.GridSize.x;
        layoutGroup.spacing = Vector2.one * tileSpacing;
        layoutGroup.cellSize = Vector2.one * calculatedTileSize;
    }

    private void CreateTiles(CrosswordGridCreator gridCreator)
    {
        for (int y = 0; y < gridCreator.GridSize.y; y++)
        {
            for (int x = 0; x < gridCreator.GridSize.x; x++)
            {
                Vector2Int position = gridCreator.LowestPosition + new Vector2Int(x, y);

                if (gridCreator.Tiles.TryGetValue(position, out TileData tileData))
                {
                    CrosswordLetterTile tile = Instantiate(letterTilePrefab, layoutGroup.transform);

                    tile.Setup(tileData);
                    tile.OnTileClicked += OnTileClicked;
                    tile.OnLetterEntered += OnLetterEntered;

                    tiles.Add(position, tile);
                }
                else
                {
                    Instantiate(emptyTilePrefab, layoutGroup.transform);
                }
            }
        }
    }

    private void OnTileClicked(LetterTile tile)
    {
        SelectTile(((CrosswordLetterTile)tile).TileData.Position);
    }

    private void OnLetterEntered(LetterTile tile)
    {
        CrosswordLetterTile crosswordTile = (CrosswordLetterTile)tile;
        Vector2Int position = crosswordTile.TileData.Position;

        bool selectedWordCorrect = IsSelectedWordCorrect();

        // On vérifie les DEUX mots qui passent par cette case (horizontal et vertical),
        // pas seulement le mot sélectionné : sinon un mot complété par sa case de
        // croisement n'était jamais validé, et la grille restait bloquée.
        LockCompletedWordsAt(position);

        if (AreAllTilesLocked())
        {
            solved = true;
            ShowHints();
            OnCrosswordCompleted?.Invoke();
            return;
        }

        if (selectedWordCorrect)
        {
            LockHighlightedTiles();
            return;
        }

        SelectNextTile(position);
    }

    /// <summary>Verrouille chaque mot passant par cette case s'il est entièrement juste.</summary>
    private void LockCompletedWordsAt(Vector2Int position)
    {
        if (!tiles.TryGetValue(position, out CrosswordLetterTile tile))
            return;

        if (tile.TileData.IsHorizontal)
            LockIfCorrect(GetWordTiles(position, Vector2Int.right));

        if (tile.TileData.IsVertical)
            LockIfCorrect(GetWordTiles(position, Vector2Int.up));
    }

    private void LockIfCorrect(List<CrosswordLetterTile> word)
    {
        if (word.Count < 2)
            return;

        foreach (CrosswordLetterTile tile in word)
        {
            if (!IsTileCorrect(tile))
                return;
        }

        foreach (CrosswordLetterTile tile in word)
        {
            tile.Lock();
            tile.SetCorrect();
        }
    }

    /// <summary>Toutes les cases contiguës passant par cette position, dans une direction.</summary>
    private List<CrosswordLetterTile> GetWordTiles(Vector2Int position, Vector2Int direction)
    {
        List<CrosswordLetterTile> word = new();

        Vector2Int start = position;

        while (tiles.ContainsKey(start - direction))
            start -= direction;

        for (Vector2Int current = start; tiles.TryGetValue(current, out CrosswordLetterTile tile); current += direction)
            word.Add(tile);

        return word;
    }

    private static bool IsTileCorrect(CrosswordLetterTile tile)
    {
        return tile.HasLetter() && CrosswordText.SameLetter(tile.GetEnteredLetter(), tile.TileData.CorrectChar);
    }

    private void OnBackspace(InputAction.CallbackContext context)
    {
        if (CutsceneDirector.IsPlaying)
            return;

        if (solved || selectedTile == null || selectedTile.IsLocked)
            return;

        if (selectedTile.HasLetter())
        {
            selectedTile.ClearLetter();
            return;
        }

        SelectPreviousTile(selectedTile.TileData.Position);
    }

    public void SelectTile(Vector2Int position)
    {
        if (!tiles.TryGetValue(position, out CrosswordLetterTile tile))
            return;

        if (tile.IsLocked)
            return;

        if (selectedTile == tile)
        {
            if (tile.TileData.IsHorizontal && tile.TileData.IsVertical)
            {
                currentSelectionIsDown = !currentSelectionIsDown;
                SelectTiles(position, currentSelectionIsDown);
            }

            return;
        }

        if (tile.TileData.IsHorizontal)
            currentSelectionIsDown = false;
        else
            currentSelectionIsDown = true;

        SelectTiles(position, currentSelectionIsDown);
    }

    private void SelectTiles(Vector2Int position, bool isDown)
    {
        foreach (CrosswordLetterTile tile in highlightedTiles)
            tile.SetBase();

        highlightedTiles.Clear();

        if (!tiles.TryGetValue(position, out CrosswordLetterTile startTile))
            return;

        Vector2Int direction = isDown ? Vector2Int.up : Vector2Int.right;
        Vector2Int currentPosition = position;

        while (tiles.TryGetValue(currentPosition, out CrosswordLetterTile tile))
        {
            highlightedTiles.Add(tile);
            currentPosition += direction;
        }

        currentPosition = position - direction;

        while (tiles.TryGetValue(currentPosition, out CrosswordLetterTile tile))
        {
            highlightedTiles.Add(tile);
            currentPosition -= direction;
        }

        foreach (CrosswordLetterTile tile in highlightedTiles)
            tile.SetHighlighted();

        selectedTile = startTile;
        selectedTile.SetSelected();
    }

    public void SelectNextTile(Vector2Int position)
    {
        Vector2Int direction = currentSelectionIsDown ? Vector2Int.up : Vector2Int.right;
        SelectTileInDirection(position, direction);
    }

    public void SelectPreviousTile(Vector2Int position)
    {
        Vector2Int direction = currentSelectionIsDown ? Vector2Int.down : Vector2Int.left;
        SelectTileInDirection(position, direction);
    }

    private void SelectTileInDirection(Vector2Int position, Vector2Int direction)
    {
        Vector2Int targetPosition = position + direction;

        while (tiles.TryGetValue(targetPosition, out CrosswordLetterTile targetTile))
        {
            if (!targetTile.IsLocked)
            {
                if (selectedTile != null)
                    selectedTile.SetHighlighted();

                selectedTile = targetTile;
                selectedTile.SetSelected();
                targetTile.Focus();
                return;
            }

            targetPosition += direction;
        }
    }

    private bool IsSelectedWordCorrect()
    {
        foreach (CrosswordLetterTile tile in highlightedTiles)
        {
            if (!IsTileCorrect(tile))
                return false;
        }

        return true;
    }

    private void LockHighlightedTiles()
    {
        foreach (CrosswordLetterTile tile in highlightedTiles)
        {
            tile.Lock();
            tile.SetCorrect();
        }

        highlightedTiles.Clear();
        selectedTile = null;
    }

    private bool AreAllTilesLocked()
    {
        foreach (CrosswordLetterTile tile in tiles.Values)
        {
            if (!tile.IsLocked)
                return false;
        }

        return true;
    }

    public void SaveState(CrosswordState state)
    {
        state.tiles.Clear();

        foreach (CrosswordLetterTile tile in tiles.Values)
        {
            state.tiles.Add(new CrosswordState.CrosswordTileState
            {
                position = tile.TileData.Position,
                letter = tile.HasLetter() ? tile.GetEnteredLetter() : '\0',
                locked = tile.IsLocked
            });
        }
    }

    public void RestoreState(CrosswordState state)
    {
        foreach (CrosswordState.CrosswordTileState tileState in state.tiles)
        {
            if (!tiles.TryGetValue(tileState.position, out CrosswordLetterTile tile))
                continue;

            if (tileState.letter != '\0')
                tile.SetLetter(tileState.letter);

            if (tileState.locked)
            {
                tile.Lock();
                tile.SetCorrect();
            }
        }
    }

    public void AutoSolve()
    {
        if (solved)
            return;
        Solve();
        solved = true;
        ShowHints();
        OnCrosswordCompleted?.Invoke();
    }

    public void Solve()
    {
        solved = true;

        foreach (CrosswordLetterTile tile in tiles.Values)
        {
            tile.SetLetter(tile.TileData.CorrectChar);
            tile.Lock();
            tile.ShowHint();
        }
    }

    private void ShowHints()
    {
        foreach(CrosswordLetterTile tile in tiles.Values)
        {
            tile.ShowHint();
        }
    }
}