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
    [SerializeField] private CrosswordLetterTile letterTile;
    [SerializeField] private GameObject emptyTile;
    [SerializeField] private RectTransform grid;
    [SerializeField] private InputActionReference backspaceAction;

    [Header("Grid Settings")]
    [SerializeField] private float tileSpacing = 5;

    private Dictionary<Vector2Int, LetterTile> tiles;
    private HashSet<LetterTile> selectedTiles;
    private LetterTile selectedTile;

    private bool currentSelectionIsDown = true;

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
        tiles = new Dictionary<Vector2Int, LetterTile>();
        selectedTiles = new HashSet<LetterTile>();
        selectedTile = null;
    }

    private void ConfigureGridLayout(CrosswordGridCreator gridCreator)
    {
        float availableWidth = grid.rect.width - (gridCreator.GridSize.x - 1) * tileSpacing;
        float availableHeight = grid.rect.height - (gridCreator.GridSize.y - 1) * tileSpacing;

        float calculatedTileSize = Mathf.Min(
            availableWidth / gridCreator.GridSize.x,
            availableHeight / gridCreator.GridSize.y
        );

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
                    CrosswordLetterTile tile = Instantiate(letterTile, layoutGroup.transform);

                    tile.Setup(tileData);
                    tile.OnTileClicked += OnTileClicked;
                    tile.OnLetterEntered += OnLetterEntered;

                    tiles.Add(position, tile);
                }
                else
                {
                    Instantiate(emptyTile, layoutGroup.transform);
                }
            }
        }
    }

    private void OnTileClicked(LetterTile tile)
    {
        if (tile is not CrosswordLetterTile crosswordTile)
            return;

        SelectTile(crosswordTile.TileData.Position);
    }

    private void OnLetterEntered(LetterTile tile)
    {
        if (tile is not CrosswordLetterTile crosswordTile)
            return;

        SelectNextTile(crosswordTile.TileData.Position);
    }

    private void OnBackspace(InputAction.CallbackContext context)
    {
        if (selectedTile == null || selectedTile.IsLocked)
            return;

        if (selectedTile.HasLetter())
        {
            selectedTile.ClearLetter();
            return;
        }

        if (selectedTile is CrosswordLetterTile crosswordTile)
            SelectPreviousTile(crosswordTile.TileData.Position);
    }

    public void SelectTile(Vector2Int position)
    {
        if (!tiles.TryGetValue(position, out LetterTile tile))
            return;

        if (tile is not CrosswordLetterTile crosswordTile)
            return;

        if (selectedTile == tile)
        {
            if (crosswordTile.TileData.IsHorizontal &&
                crosswordTile.TileData.IsVertical)
            {
                currentSelectionIsDown = !currentSelectionIsDown;
                SelectTiles(position, currentSelectionIsDown);
            }

            return;
        }

        if (selectedTile != null)
            selectedTile.SetHighlighted();

        if (currentSelectionIsDown && !crosswordTile.TileData.IsVertical)
            currentSelectionIsDown = false;

        if (!currentSelectionIsDown && !crosswordTile.TileData.IsHorizontal)
            currentSelectionIsDown = true;

        SelectTiles(position, currentSelectionIsDown);
    }

    private void SelectTiles(Vector2Int position, bool isDown)
    {
        foreach (LetterTile tile in selectedTiles)
            tile.SetBase();

        selectedTiles.Clear();

        if (!tiles.TryGetValue(position, out LetterTile startTile))
            return;

        Vector2Int direction = isDown ? Vector2Int.up : Vector2Int.right;
        Vector2Int currentPosition = position;

        while (tiles.TryGetValue(currentPosition, out LetterTile tile))
        {
            if (tile is not CrosswordLetterTile crosswordTile)
                break;

            if ((isDown && !crosswordTile.TileData.IsVertical) ||
                (!isDown && !crosswordTile.TileData.IsHorizontal))
                break;

            selectedTiles.Add(tile);
            currentPosition += direction;
        }

        currentPosition = position - direction;

        while (tiles.TryGetValue(currentPosition, out LetterTile tile))
        {
            if (tile is not CrosswordLetterTile crosswordTile)
                break;

            if ((isDown && !crosswordTile.TileData.IsVertical) ||
                (!isDown && !crosswordTile.TileData.IsHorizontal))
                break;

            selectedTiles.Add(tile);
            currentPosition -= direction;
        }

        foreach (LetterTile selectedTile in selectedTiles)
            selectedTile.SetHighlighted();

        selectedTile = startTile;
        selectedTile.SetSelected();
    }

    public void SelectNextTile(Vector2Int position)
    {
        Debug.Log("next tile");

        if (IsSelectedWordCorrect())
        {
            LockSelectedTiles();

            if (AreAllTilesLocked())
                OnCrosswordCompleted?.Invoke();

            return;
        }

        Vector2Int direction = currentSelectionIsDown
            ? Vector2Int.up
            : Vector2Int.right;

        Vector2Int nextPosition = position + direction;

        while (tiles.TryGetValue(nextPosition, out LetterTile nextTile))
        {
            if (!nextTile.IsLocked)
            {
                selectedTile.SetHighlighted();
                selectedTile = nextTile;
                selectedTile.SetSelected();
                nextTile.Focus();
                return;
            }

            nextPosition += direction;
        }
    }

    public void SelectPreviousTile(Vector2Int position)
    {
        Vector2Int direction = currentSelectionIsDown
            ? Vector2Int.up
            : Vector2Int.right;

        Vector2Int previousPosition = position - direction;

        while (tiles.TryGetValue(previousPosition, out LetterTile previousTile))
        {
            if (!previousTile.IsLocked)
            {
                selectedTile.SetHighlighted();
                selectedTile = previousTile;
                selectedTile.SetSelected();
                previousTile.Focus();
                return;
            }

            previousPosition -= direction;
        }
    }

    private bool IsSelectedWordCorrect()
    {
        foreach (LetterTile tile in selectedTiles)
        {
            if (tile is not CrosswordLetterTile crosswordTile)
                return false;

            if (!tile.HasLetter() ||
                tile.GetEnteredLetter() != crosswordTile.TileData.CorrectChar)
                return false;
        }

        return true;
    }

    private void LockSelectedTiles()
    {
        foreach (LetterTile tile in selectedTiles)
        {
            tile.Lock();
            tile.SetCorrect();
        }

        selectedTiles.Clear();
        selectedTile = null;
    }

    private bool AreAllTilesLocked()
    {
        foreach (LetterTile tile in tiles.Values)
        {
            if (!tile.IsLocked)
                return false;
        }

        return true;
    }

    public void SaveState(CrosswordState state)
    {
        state.tiles.Clear();

        foreach (LetterTile tile in tiles.Values)
        {
            CrosswordLetterTile crosswordTile = (CrosswordLetterTile)tile;

            state.tiles.Add(new CrosswordState.CrosswordTileState
            {
                position = crosswordTile.TileData.Position,
                letter = crosswordTile.HasLetter() ? crosswordTile.GetEnteredLetter() : '\0',
                locked = crosswordTile.IsLocked
            });
        }
    }

    public void RestoreState(CrosswordState state)
    {
        foreach (CrosswordState.CrosswordTileState tileState in state.tiles)
        {
            if (!tiles.TryGetValue(tileState.position, out LetterTile tile))
                continue;

            CrosswordLetterTile crosswordTile = (CrosswordLetterTile)tile;

            if (tileState.letter != '\0')
                crosswordTile.SetLetter(tileState.letter);

            if (tileState.locked)
            {
                crosswordTile.Lock();
                crosswordTile.SetCorrect();
            }
        }
    }

    public void Solve()
    {
        foreach (LetterTile tile in tiles.Values)
        {
            CrosswordLetterTile crosswordTile = (CrosswordLetterTile)tile;

            crosswordTile.SetLetter(crosswordTile.TileData.CorrectChar);
            crosswordTile.Lock();
            crosswordTile.SetCorrect();
        }
    }
}