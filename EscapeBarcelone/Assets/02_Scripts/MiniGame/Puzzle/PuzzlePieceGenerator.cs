using System.Collections.Generic;
using UnityEngine;

public class PuzzlePieceGenerator
{
    private readonly PuzzlePiece piecePrefab;
    private readonly Transform piecesParent;
    private readonly PuzzleConnectionManager connectionManager;

    public List<PuzzlePieceGroup> groups { get; private set; } = new();

    public PuzzlePieceGenerator(PuzzlePiece piecePrefab, Transform piecesParent, PuzzleConnectionManager connectionManager)
    {
        this.piecePrefab = piecePrefab;
        this.piecesParent = piecesParent;
        this.connectionManager = connectionManager;
    }

    public void Generate(Sprite sprite, PuzzleGridData grid, int sortingOrder, Bounds shuffleArea)
    {
        Texture2D texture = sprite.texture;
        Rect spriteRect = sprite.rect;

        for (int y = 0; y < grid.rows; y++)
        {
            for (int x = 0; x < grid.columns; x++)
            {
                CreatePiece(texture, spriteRect, x, y, grid, sprite.pixelsPerUnit, sortingOrder, shuffleArea);
            }
        }

        connectionManager.SetGroupCount(groups.Count);
    }

    private void CreatePiece(Texture2D texture, Rect spriteRect, int x, int y, PuzzleGridData grid, float ppu, int sortingOrder, Bounds shuffleArea)
    {
        Sprite pieceSprite = CreateSprite(texture, spriteRect, x, y, grid, ppu);

        PuzzlePiece piece = Object.Instantiate(piecePrefab, piecesParent);
        piece.Initialize(pieceSprite, new Vector2Int(x, y), connectionManager, sortingOrder, shuffleArea);

        CreateGroup(piece);

        connectionManager.Register(piece);
    }

    private Sprite CreateSprite(Texture2D texture, Rect spriteRect, int x, int y, PuzzleGridData grid, float pixelsPerUnit)
    {
        float cellWidth = spriteRect.width / grid.columns;
        float cellHeight = spriteRect.height / grid.rows;

        int xMin = Mathf.FloorToInt(x * cellWidth);
        int xMax = Mathf.FloorToInt((x + 1) * cellWidth);
        int yMin = Mathf.FloorToInt(y * cellHeight);
        int yMax = Mathf.FloorToInt((y + 1) * cellHeight);

        Rect rect = new Rect(spriteRect.x + xMin, spriteRect.y + yMin, xMax - xMin, yMax - yMin);

        return Sprite.Create(texture, rect, Vector2.one * 0.5f, pixelsPerUnit);
    }

    private void CreateGroup(PuzzlePiece piece)
    {
        GameObject groupObject = new GameObject("Puzzle Group");
        groupObject.transform.SetParent(piecesParent);

        PuzzlePieceGroup group = groupObject.AddComponent<PuzzlePieceGroup>();
        group.AddPiece(piece);
        groups.Add(group);

        piece.transform.localPosition = Vector3.zero;
    }
}