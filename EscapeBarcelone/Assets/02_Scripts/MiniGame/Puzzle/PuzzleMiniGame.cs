using System.Collections.Generic;
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

    [SerializeField] private GameObject completedSprite;

    private PuzzlePieceGenerator pieceGenerator;
    private PuzzleConnectionManager connectionManager;
    private PuzzleShuffle shuffleManager;
    private PuzzleHidePiece hidePiece;

    private bool isCompleted;

    private void Awake()
    {
        connectionManager = new PuzzleConnectionManager();
        pieceGenerator = new PuzzlePieceGenerator(piecePrefab, piecesParent, connectionManager);
        shuffleManager = new PuzzleShuffle(shuffleArea.bounds, centerHole.bounds);
        hidePiece = new PuzzleHidePiece();
    }

    protected override void Initialize()
    {
        GeneratePuzzle();
        InitializePuzzleState();
        RevealHiddenPieces();

        connectionManager.PuzzleCompleted += CompleteGame;
    }

    private void OnDisable()
    {
        if (isCompleted)
            return;

        SavePuzzleGroups(ServiceLocator.Get<WorldState>().puzzleState);
    }

    private void GeneratePuzzle()
    {
        PuzzleGridData gridData = PuzzleGridGenerator.CalculateGrid(size, sprite.texture.width, sprite.texture.height);
        pieceGenerator.Generate(sprite, gridData, piecesSortingOrder, shuffleArea.bounds);
        shuffleManager.Shuffle(pieceGenerator.groups);
    }

    private void InitializePuzzleState()
    {
        PuzzleState puzzleState = ServiceLocator.Get<WorldState>().puzzleState;

        if (!puzzleState.initialized)
            CreatePuzzleState(puzzleState);
        else
            RestorePuzzleState(puzzleState);
    }

    private void CreatePuzzleState(PuzzleState puzzleState)
    {
        hidePiece.HidePieces(pieceGenerator.groups, pieceMissing);

        foreach (PuzzlePieceGroup group in hidePiece.hiddenPieces)
            puzzleState.hiddenPieces.Add(group.Pieces[0].GridPosition);

        SavePuzzleGroups(puzzleState);

        puzzleState.initialized = true;
    }

    private void RestorePuzzleState(PuzzleState puzzleState)
    {
        foreach (PuzzlePieceGroup group in pieceGenerator.groups)
        {
            if (!puzzleState.hiddenPieces.Contains(group.Pieces[0].GridPosition))
                continue;

            group.gameObject.SetActive(false);
            hidePiece.hiddenPieces.Add(group);
        }

        foreach (PuzzleState.PuzzleGroupState groupState in puzzleState.groupStates)
            connectionManager.RestoreGroup(groupState);
    }

    private void SavePuzzleGroups(PuzzleState puzzleState)
    {
        puzzleState.groupStates.Clear();

        foreach (PuzzlePieceGroup group in pieceGenerator.groups)
        {
            if (group == null)
                continue;

            PuzzleState.PuzzleGroupState groupState = new PuzzleState.PuzzleGroupState
            {
                position = group.transform.position
            };

            foreach (PuzzlePiece piece in group.Pieces)
                groupState.pieces.Add(piece.GridPosition);

            puzzleState.groupStates.Add(groupState);
        }
    }

    private void RevealHiddenPieces()
    {
        int piecesCollected = ServiceLocator.Get<WorldState>().puzzlePiecesCollected.Count;
        hidePiece.RevealHiddenPiece(piecesCollected);
    }

    private void CompleteGame()
    {
        connectionManager.PuzzleCompleted -= CompleteGame;
        ApplyCompletedState();
        TriggerGameCompleted();
    }

    private void ApplyCompletedState()
    {
        isCompleted = true;
        completedSprite.SetActive(true);
        Destroy(piecesParent.gameObject);
    }

    protected override void SolveMiniGame()
    {
        ApplyCompletedState();
    }
}

[System.Serializable]
public class PuzzleState
{
    public bool initialized;
    public List<PuzzleGroupState> groupStates = new();
    public List<Vector2Int> hiddenPieces = new();

    [System.Serializable]
    public class PuzzleGroupState
    {
        public Vector3 position;
        public List<Vector2Int> pieces = new();
    }
}