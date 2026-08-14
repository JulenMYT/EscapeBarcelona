using UnityEngine;

public class PuzzleMiniGame : MiniGame
{
    [SerializeField] private Sprite sprite;
    [SerializeField] private int size = 100;
    [SerializeField] private int pieceMissing = 0;
    [SerializeField] private int piecesSortingOrder = 11;

    [SerializeField] private PuzzlePiece piecePrefab;
    [SerializeField] private Transform piecesParent;

    [SerializeField] private BoxCollider2D shuffleArea;
    [SerializeField] private BoxCollider2D centerHole;

    [SerializeField] private SpriteRenderer completedSprite;

    private PuzzlePieceGenerator pieceGenerator;
    private PuzzleConnectionManager connectionManager;
    private PuzzleShuffle shuffleManager;
    private PuzzleHidePiece hidePiece;

    private void Awake()
    {
        connectionManager = new PuzzleConnectionManager();
        pieceGenerator = new PuzzlePieceGenerator(piecePrefab, piecesParent, connectionManager);
        shuffleManager = new PuzzleShuffle(shuffleArea.bounds, centerHole.bounds);
    }

    protected override void Initialize()
    {
        PuzzleGridData gridData = PuzzleGridGenerator.CalculateGrid(size, sprite.texture.width, sprite.texture.height);
        pieceGenerator.Generate(sprite, gridData, piecesSortingOrder, shuffleArea.bounds);
        shuffleManager.Shuffle(pieceGenerator.groups);

        connectionManager.PuzzleCompleted += CompleteGame;
    }

    private void CompleteGame()
    {
        connectionManager.PuzzleCompleted -= CompleteGame;

        ApplyCompletedState();
        TriggerGameCompleted();
    }

    private void ApplyCompletedState()
    {
        completedSprite.sprite = sprite;
        completedSprite.gameObject.SetActive(true);

        Destroy(piecesParent.gameObject);
    }

    protected override void SolveMiniGame()
    {
        ApplyCompletedState();
    }
}