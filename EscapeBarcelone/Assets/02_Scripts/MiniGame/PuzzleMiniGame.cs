using UnityEngine;

public class PuzzleMiniGame : MonoBehaviour
{
    [SerializeField] private Sprite sprite;
    [SerializeField] private int size = 100;

    [SerializeField] private PuzzlePiece piecePrefab;
    [SerializeField] private Transform piecesParent;

    [SerializeField] private BoxCollider2D centerHole;
    [SerializeField] private BoxCollider2D shuffleArea;

    private PuzzlePieceGenerator pieceGenerator;
    private PuzzleConnectionManager connectionManager = new();
    private PuzzleShuffleManager shuffleManager;

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

        shuffleArea.enabled = false;
        centerHole.enabled = false;
    }

    private void Start()
    {
        CreatePuzzle();
        piecesParent.gameObject.SetActive(false);
        StartMiniGame(); //testing
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
    }

    public void StartMiniGame()
    {
        piecesParent.gameObject.SetActive( true );
    }
}