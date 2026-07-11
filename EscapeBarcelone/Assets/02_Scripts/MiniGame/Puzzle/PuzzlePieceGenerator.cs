using System.Collections.Generic;
using UnityEngine;

public class PuzzlePieceGenerator
{
    private readonly PuzzlePiece piecePrefab;
    private readonly Transform piecesParent;
    private readonly PuzzleConnectionManager connectionManager;

    private List<PuzzlePieceGroup> groups = new();

    public IReadOnlyList<PuzzlePieceGroup> Groups => groups;

    public PuzzlePieceGenerator(
        PuzzlePiece piecePrefab,
        Transform piecesParent,
        PuzzleConnectionManager connectionManager)
    {
        this.piecePrefab = piecePrefab;
        this.piecesParent = piecesParent;
        this.connectionManager = connectionManager;
    }

    public void Generate(Sprite sprite, PuzzleGridData grid)
    {
        Texture2D texture = sprite.texture;
        Rect spriteRect = sprite.rect;


        for (int y = 0; y < grid.rows; y++)
        {
            for (int x = 0; x < grid.columns; x++)
            {
                CreatePiece(
                    texture,
                    spriteRect,
                    x,
                    y,
                    grid,
                    sprite.pixelsPerUnit
                );
            }
        }
    }

    private void CreatePiece(
        Texture2D texture,
        Rect spriteRect,
        int x,
        int y,
        PuzzleGridData grid,
        float ppu)
    {
        Sprite pieceSprite = CreateSprite(
            texture,
            spriteRect,
            x,
            y,
            grid,
            ppu
        );

        PuzzlePiece piece = Object.Instantiate(
            piecePrefab,
            piecesParent
        );

        Vector2Int gridPosition = new Vector2Int(x, y);

        piece.Initialize(
            pieceSprite,
            gridPosition,
            connectionManager
        );

        CreateGroup(piece);

        connectionManager.Register(piece);

        //piece.transform.localPosition =
        //    GetPiecePosition(pieceSprite, x, y);
    }

    private Sprite CreateSprite(Texture2D texture, Rect spriteRect, int x, int y, PuzzleGridData grid, float pixelsPerUnit)
    {
        Rect rect = new Rect(
            spriteRect.x + x * grid.pieceWidth,
            spriteRect.y + y * grid.pieceHeight,
            grid.pieceWidth,
            grid.pieceHeight
        );

        return Sprite.Create(
            texture,
            rect,
            Vector2.one * 0.5f,
            pixelsPerUnit
        );
    }

    private void CreateGroup(PuzzlePiece piece)
    {
        GameObject groupObject =
            new GameObject("Puzzle Group");


        groupObject.transform.SetParent(
            piecesParent
        );


        PuzzlePieceGroup group =
            groupObject.AddComponent<PuzzlePieceGroup>();


        group.AddPiece(piece);
        groups.Add(group);
    }

    private Vector3 GetPiecePosition(
        Sprite sprite,
        int x,
        int y)
    {
        return new Vector3(
            x * sprite.bounds.size.x,
            y * sprite.bounds.size.y,
            0
        );
    }
}