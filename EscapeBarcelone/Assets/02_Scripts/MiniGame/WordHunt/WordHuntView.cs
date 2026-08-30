using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class WordHuntView : MonoBehaviour
{
    [SerializeField] private GridLayoutGroup grid;
    [SerializeField] private WordHuntCell cellPrefab;
    [SerializeField] private RectTransform selectionVisual;

    [SerializeField] private InputActionReference pointerAction;

    private bool isSelecting;

    private Vector2Int firstPosition;
    private Vector2Int currentPosition;
    private Vector2Int direction;

    private Vector2Int gridSize;
    private Dictionary<Vector2Int, WordHuntCell> cells = new();

    public event Action<Vector2Int, Vector2Int, Vector2Int> SelectionEnded;

    private void Awake()
    {
        selectionVisual.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        pointerAction.action.canceled += HandlePointerUp;
    }

    private void OnDisable()
    {
        pointerAction.action.canceled -= HandlePointerUp;
    }

    public void CreateGrid(WordHuntData data)
    {
        int width = data.Rows[0].Length;
        int height = data.Rows.Count;
        gridSize = new Vector2Int(width, height);

        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = width;

        RectTransform rectTransform = grid.GetComponent<RectTransform>();

        float cellWidth = rectTransform.rect.width / width;
        float cellHeight = rectTransform.rect.height / height;

        float cellSize = Mathf.Min(cellWidth, cellHeight);

        grid.cellSize = new Vector2(cellSize, cellSize);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2Int position = new(x, y);

                WordHuntCell cell = Instantiate(cellPrefab, grid.transform);
                cell.Setup(position, data.Rows[y][x]);

                cells.Add(position, cell);

                cell.PointerDown += HandlePointerDown;
                cell.PointerEnter += HandlePointerEnter;
            }
        }
    }

    private void HandlePointerDown(Vector2Int position)
    {
        firstPosition = position;
        currentPosition = position;
        isSelecting = true;
    }

    private void HandlePointerEnter(Vector2Int position)
    {
        if (!isSelecting)
            return;

        currentPosition = position;

        if (currentPosition == firstPosition)
            return;

        Vector2Int absoluteDirection = currentPosition - firstPosition;

        direction = CalculateDirection(absoluteDirection);

        Vector2Int finalPosition = GetFinalPosition();

        UpdateSelectionVisual(firstPosition, finalPosition);
        selectionVisual.gameObject.SetActive(true);
    }

    private void HandlePointerUp(InputAction.CallbackContext context)
    {
        if (!isSelecting)
            return;

        EndSelection();
        isSelecting = false;
    }

    private Vector2Int CalculateDirection(Vector2Int absoluteDirection)
    {
        Vector2 normalizedDirection = ((Vector2)absoluteDirection).normalized;

        Vector2Int bestDirection = Vector2Int.zero;
        float bestDot = float.MinValue;

        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                if (x == 0 && y == 0)
                    continue;

                Vector2Int testDirection = new(x, y);
                float testDot = Vector2.Dot(normalizedDirection, ((Vector2)testDirection).normalized);

                if (testDot > bestDot)
                {
                    bestDirection = testDirection;
                    bestDot = testDot;
                }
            }
        }

        return bestDirection;
    }

    private void EndSelection()
    {
        Vector2Int finalPosition = GetFinalPosition();

        SelectionEnded?.Invoke(firstPosition, finalPosition, direction);
    }

    private Vector2Int GetFinalPosition()
    {
        Vector2Int finalPosition = firstPosition;
        float closestDistance = float.MaxValue;

        Vector2Int position = firstPosition;

        while (position.x >= 0 && position.x < gridSize.x && position.y >= 0 && position.y < gridSize.y)
        {
            float distance = Vector2.Distance(position, currentPosition);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                finalPosition = position;
            }

            position += direction;
        }

        return finalPosition;
    }

    private void UpdateSelectionVisual(Vector2Int start, Vector2Int end)
    {
        Vector2 startPosition = cells[start].Position;
        Vector2 endPosition = cells[end].Position;

        Vector2 difference = endPosition - startPosition;

        selectionVisual.position = (startPosition + endPosition) * 0.5f;
        selectionVisual.sizeDelta = new Vector2(difference.magnitude + grid.cellSize.x, grid.cellSize.y);

        selectionVisual.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg);
    }
}