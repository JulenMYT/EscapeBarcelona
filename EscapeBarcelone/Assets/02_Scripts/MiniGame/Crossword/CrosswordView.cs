using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// La grille des mots croisés : crée les cases, gère la sélection (clic, définition,
/// clavier), vérifie les mots tapés et les verrouille quand ils sont justes.
/// Le CrosswordManager la pilote et écoute ses événements.
/// </summary>
public class CrosswordView : MonoBehaviour
{
    public event Action OnCrosswordCompleted;

    /// <summary>
    /// Le mot sélectionné a changé : première case du mot et direction. Null quand plus rien
    /// n'est sélectionné (mot verrouillé, grille finie).
    /// </summary>
    public event Action<Vector2Int?, bool> SelectedWordChanged;

    /// <summary>Des cases viennent d'être verrouillées (un mot vient d'être trouvé).</summary>
    public event Action TilesLocked;

    [Header("References")]
    [SerializeField] private GridLayoutGroup layoutGroup;
    [SerializeField] private CrosswordLetterTile letterTilePrefab;
    [SerializeField] private GameObject emptyTilePrefab;
    [SerializeField] private RectTransform grid;
    [SerializeField] private InputActionReference backspaceAction;

    [Header("Grid Settings")]
    [SerializeField] private float tileSpacing = 5;

    [Tooltip("Taille minimale d'une case (px). Si la grille est trop grande pour son cadre avec " +
             "des cases de cette taille, elle dépasse et on la fait défiler (molette ou glisser).")]
    [SerializeField] private float minTileSize = 55;

    private Dictionary<Vector2Int, CrosswordLetterTile> tiles;
    private HashSet<CrosswordLetterTile> highlightedTiles;
    private CrosswordLetterTile selectedTile;

    private bool currentSelectionIsDown = true;

    private CrosswordGridScroller scroller;

    public bool solved { get; private set; }

    private void OnEnable()
    {
        backspaceAction.action.performed += OnBackspace;
    }

    private void OnDisable()
    {
        backspaceAction.action.performed -= OnBackspace;
    }

    // ───────────────────────── Création de la grille ─────────────────────────

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
        Vector2Int size = gridCreator.GridSize;
        Vector2 available = Scroller().AvailableSize;

        float availableWidth = available.x - (size.x - 1) * tileSpacing;
        float availableHeight = available.y - (size.y - 1) * tileSpacing;

        // Les cases remplissent le cadre si possible, sans descendre sous la taille minimale :
        // une grande grille dépasse alors du cadre et défile, au lieu d'avoir des cases minuscules.
        float fittedTileSize = Mathf.Min(availableWidth / size.x, availableHeight / size.y);
        float tileSize = Mathf.Max(fittedTileSize, minTileSize);

        layoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layoutGroup.constraintCount = size.x;
        layoutGroup.spacing = Vector2.one * tileSpacing;
        layoutGroup.cellSize = Vector2.one * tileSize;

        scroller.SetContentSize(new Vector2(
            size.x * tileSize + (size.x - 1) * tileSpacing,
            size.y * tileSize + (size.y - 1) * tileSpacing));
    }

    /// <summary>Le défilement, posé sur le cadre de la grille (son parent, le masque).</summary>
    private CrosswordGridScroller Scroller()
    {
        if (scroller != null)
            return scroller;

        RectTransform content = grid;
        GameObject viewport = content.parent.gameObject;

        if (!viewport.TryGetComponent(out scroller))
            scroller = viewport.AddComponent<CrosswordGridScroller>();

        scroller.Initialize(content);
        return scroller;
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

                    // Mot donné au joueur : déjà écrit, juste et verrouillé.
                    if (tileData.IsPrefilled)
                    {
                        tile.SetLetter(tileData.CorrectChar);
                        tile.Lock();
                        tile.SetCorrect();
                    }
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

    // ───────────────────────── Sélection ─────────────────────────

    private void OnTileClicked(LetterTile tile)
    {
        SelectTile(((CrosswordLetterTile)tile).TileData.Position);
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

    /// <summary>
    /// Sélectionne le mot qui commence à cette case (clic sur une définition) : sa première
    /// case libre reçoit le curseur. Un mot déjà trouvé n'est pas sélectionnable.
    /// </summary>
    public void SelectWord(Vector2Int origin, bool isDown)
    {
        if (solved)
            return;

        Vector2Int direction = isDown ? Vector2Int.up : Vector2Int.right;

        for (Vector2Int position = origin; tiles.TryGetValue(position, out CrosswordLetterTile tile); position += direction)
        {
            if (tile.IsLocked)
                continue;

            currentSelectionIsDown = isDown;
            SelectTiles(position, isDown);
            tile.Focus();
            return;
        }
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
        scroller?.EnsureVisible((RectTransform)selectedTile.transform);

        SelectedWordChanged?.Invoke(WordStart(position, direction), isDown);
    }

    /// <summary>Première case du mot qui passe par cette position, dans cette direction.</summary>
    private Vector2Int WordStart(Vector2Int position, Vector2Int direction)
    {
        while (tiles.ContainsKey(position - direction))
            position -= direction;

        return position;
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
                scroller?.EnsureVisible((RectTransform)selectedTile.transform);
                return;
            }

            targetPosition += direction;
        }
    }

    // ───────────────────────── Saisie ─────────────────────────

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
            SelectedWordChanged?.Invoke(null, false);
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

    // ───────────────────────── Vérification des mots ─────────────────────────

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
        SelectedWordChanged?.Invoke(null, false);
        TilesLocked?.Invoke();
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

        TilesLocked?.Invoke();
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

    /// <summary>Toutes les cases de ce mot sont-elles verrouillées (mot trouvé, ou pré-rempli) ?</summary>
    public bool IsWordLocked(Vector2Int origin, bool isDown)
    {
        if (tiles == null)
            return false;

        Vector2Int direction = isDown ? Vector2Int.up : Vector2Int.right;
        List<CrosswordLetterTile> word = GetWordTiles(origin, direction);

        return word.Count > 0 && word.TrueForAll(tile => tile.IsLocked);
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

    // ───────────────────────── Grille terminée ─────────────────────────

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

    // Test uniquement : clic droit sur le composant CrosswordView > Résoudre la grille (test).
    [ContextMenu("Résoudre la grille (test)")]
    public void AutoSolve()
    {
        if (solved)
            return;
        Solve();
        solved = true;
        ShowHints();
        OnCrosswordCompleted?.Invoke();
    }

    // ───────────────────────── Sauvegarde ─────────────────────────

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
}