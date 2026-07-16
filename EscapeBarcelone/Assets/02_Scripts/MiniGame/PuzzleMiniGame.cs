using UnityEngine;

public class PuzzleMiniGame : MonoBehaviour
{
    [SerializeField] private Sprite sprite;
    [SerializeField] private int size = 100;

    [SerializeField] private PuzzlePiece piecePrefab;
    [SerializeField] private Transform piecesParent;

    [SerializeField] private BoxCollider2D centerHole;
    [SerializeField] private BoxCollider2D shuffleArea;

    [SerializeField] private InteractionElementBase finalObject;

    private PuzzlePieceGenerator pieceGenerator;
    private PuzzleConnectionManager connectionManager = new();
    private PuzzleShuffleManager shuffleManager;
    private PuzzleHidePiece hidePiece = new();
    private bool puzzleCompleted = false;

    [SerializeField] private int pieceMissing = 0;

    private void Awake()
    {
        pieceGenerator = new PuzzlePieceGenerator(
            piecePrefab,
            piecesParent,
            connectionManager
        );

        shuffleManager = new PuzzleShuffleManager(
            shuffleArea.bounds,
            centerHole.bounds
        );

        connectionManager.PuzzleCompleted += OnPuzzleCompleted;

        shuffleArea.enabled = false;
        centerHole.enabled = false;
    }

    private void Start()
    {
        CreatePuzzle();
    }

    private void OnEnable()
    {
        RefreshVisualState();
    }

    public void CreatePuzzle()
    {
        PuzzleGridData grid = PuzzleGridGenerator.CalculateGrid(
            size,
            Mathf.RoundToInt(sprite.rect.width),
            Mathf.RoundToInt(sprite.rect.height)
        );

        Debug.Log($"Grid : {grid.columns} x {grid.rows}");
        Debug.Log($"Piece size : {grid.pieceWidth} x {grid.pieceHeight}");

        pieceGenerator.Generate(sprite, grid);

        shuffleManager.Shuffle(
            pieceGenerator.Groups
        );

        hidePiece.HidePieces(pieceGenerator.Groups, pieceMissing);
    }

    public void StartMiniGame()
    {
        piecesParent.gameObject.SetActive( true );
    }

    private void OnDestroy()
    {
        connectionManager.PuzzleCompleted -= OnPuzzleCompleted;
    }

    private void OnPuzzleCompleted()
    {
        puzzleCompleted = true;
        connectionManager.PuzzleCompleted -= OnPuzzleCompleted;
        StoryManager.SetFlag(StoryFlag.hasCompletedJigsawPuzzle);
        RefreshVisualState();
    }

    private void RefreshVisualState()
    {
        piecesParent.gameObject.SetActive(!puzzleCompleted);
        if (puzzleCompleted)
            finalObject.Open();
    }

    public void RevealPiece()
    {
        hidePiece.RevealHiddenPiece();
    }
}