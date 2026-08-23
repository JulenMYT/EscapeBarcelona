using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CrosswordView : MonoBehaviour
{
    [SerializeField] private GridLayoutGroup layoutGroup;
    [SerializeField] private LetterTile letterTile;
    [SerializeField] private GameObject emptyTile;
    [SerializeField] private InputActionReference backspaceAction;

    private Dictionary<Vector2Int, LetterTile> tiles;
    private HashSet<LetterTile> selectedTiles;
    private LetterTile selectedTile;

    private bool currentSelectionIsDown = true;

    public void GenerateGrid(CrosswordGridCreator gridCreator)
    {
        tiles = new Dictionary<Vector2Int, LetterTile>();
        selectedTiles = new HashSet<LetterTile>();

        layoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layoutGroup.constraintCount = gridCreator.GridSize.x;

        for (int y = 0; y < gridCreator.GridSize.y; y++)
        {
            for (int x = 0; x < gridCreator.GridSize.x; x++)
            {
                Vector2Int position = gridCreator.LowestPosition + new Vector2Int(x, y);

                if (gridCreator.Tiles.TryGetValue(position, out TileData tileData))
                {
                    LetterTile tile = Instantiate(letterTile, layoutGroup.transform);
                    tile.Setup(tileData, this);
                    tiles.Add(position, tile);
                }
                else
                {
                    Instantiate(emptyTile, layoutGroup.transform);
                }
            }
        }
    }

    private void OnEnable()
    {
        backspaceAction.action.performed += OnBackspace;
    }

    private void OnDisable()
    {
        backspaceAction.action.performed -= OnBackspace;
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

        SelectPreviousTile(selectedTile.TileData.Position);
    }

    public void SelectTile(Vector2Int position)
    {
        if (!tiles.TryGetValue(position, out LetterTile tile))
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

        if (selectedTile != null)
            selectedTile.SetHighlighted();

        if (currentSelectionIsDown && !tile.TileData.IsVertical)
            currentSelectionIsDown = false;

        if (!currentSelectionIsDown && !tile.TileData.IsHorizontal)
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
            if ((isDown && !tile.TileData.IsVertical) || (!isDown && !tile.TileData.IsHorizontal))
                break;

            selectedTiles.Add(tile);
            currentPosition += direction;
        }

        currentPosition = position - direction;

        while (tiles.TryGetValue(currentPosition, out LetterTile tile))
        {
            if ((isDown && !tile.TileData.IsVertical) || (!isDown && !tile.TileData.IsHorizontal))
                break;

            selectedTiles.Add(tile);
            currentPosition -= direction;
        }

        foreach (LetterTile tile in selectedTiles)
            tile.SetHighlighted();

        selectedTile = tiles[position];
        selectedTile.SetSelected();
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

    public void SelectNextTile(Vector2Int position)
    {
        if (IsSelectedWordCorrect())
        {
            foreach (LetterTile tile in selectedTiles)
            {
                tile.Lock();
                tile.SetCorrect();
            }

            selectedTiles.Clear();
            selectedTile = null;
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

    private bool IsSelectedWordCorrect()
    {
        foreach (LetterTile tile in selectedTiles)
        {
            if (!tile.HasLetter())
                return false;

            if (tile.GetEnteredLetter() != tile.TileData.CorrectChar)
                return false;
        }

        return true;
    }
}