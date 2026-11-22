using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Le mini-jeu des mots croisés : relie les données (CrosswordData), la grille
/// (CrosswordView), les définitions et le mot final, et sauvegarde l'avancement dans WorldState.
/// </summary>
public class CrosswordManager : MiniGame
{
    [Header("References")]
    [SerializeField] private CrosswordData crosswordData;
    [SerializeField] private CrosswordView crosswordView;
    [SerializeField] private CrosswordDefinition crosswordDefinition;
    [SerializeField] private CrosswordFinalAnswer crosswordFinalAnswer;

    /// <summary>La grille est entièrement remplie (le mot final reste à trouver).</summary>
    public event Action OnGridCompleted;

    private CrosswordGridCreator crosswordGridCreator;
    private bool isCompleted;
    private bool initialized;

    private void Awake()
    {
        crosswordGridCreator = new CrosswordGridCreator();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        // Rien à sauvegarder si le mini-jeu est fini, s'il n'a jamais démarré, ou si
        // Bootstrap (WorldState) est déjà détruit : c'est le cas quand on arrête le Play
        // avec les mots croisés ouverts, Unity détruit les scènes dans le désordre.
        if (isCompleted || !initialized || !ServiceLocator.TryGet(out WorldState worldState))
            return;

        SaveCrosswordState(worldState.crosswordState);
    }

    private void OnDestroy()
    {
        crosswordView.OnCrosswordCompleted -= OnCrosswordCompleted;
        crosswordView.SelectedWordChanged -= OnSelectedWordChanged;
        crosswordDefinition.WordClicked -= OnDefinitionClicked;
        crosswordView.TilesLocked -= RefreshCompletedDefinitions;
        crosswordFinalAnswer.OnAnswerCompleted -= CompleteGame;
    }

    // ───────────────────────── Démarrage ─────────────────────────

    protected override void Initialize()
    {
        crosswordView.OnCrosswordCompleted += OnCrosswordCompleted;
        crosswordView.SelectedWordChanged += OnSelectedWordChanged;
        crosswordDefinition.WordClicked += OnDefinitionClicked;
        crosswordView.TilesLocked += RefreshCompletedDefinitions;
        crosswordFinalAnswer.OnAnswerCompleted += CompleteGame;

        CreateCrossword();
        InitializeCrosswordState();
        RefreshCompletedDefinitions();
        initialized = true;
    }

    private void CreateCrossword()
    {
        crosswordGridCreator.CreateTiles(crosswordData);
        crosswordDefinition.CreateDefinitions(crosswordData);
        crosswordView.GenerateGrid(crosswordGridCreator);
    }

    protected override void SolveMiniGame()
    {
        CreateCrossword();

        crosswordView.Solve();
        crosswordFinalAnswer.CreateAnswer(crosswordData);
        crosswordFinalAnswer.Solve();
        RefreshCompletedDefinitions();

        isCompleted = true;
    }

    // ───────────────────────── Grille ↔ définitions ─────────────────────────

    private void OnDefinitionClicked(WordData word)
    {
        crosswordView.SelectWord(word.originPosition, word.isDown);
    }

    private void OnSelectedWordChanged(Vector2Int? start, bool isDown)
    {
        WordData word = start.HasValue
            ? crosswordData.words.Find(w => w.originPosition == start.Value && w.isDown == isDown)
            : null;

        crosswordDefinition.Highlight(word);
    }

    /// <summary>
    /// Grise la définition de chaque mot trouvé. Appelé à l'ouverture (mots pré-remplis,
    /// état restauré) et dès qu'un mot est verrouillé dans la grille.
    /// </summary>
    private void RefreshCompletedDefinitions()
    {
        foreach (WordData word in crosswordData.words)
            crosswordDefinition.SetCompleted(word, crosswordView.IsWordLocked(word.originPosition, word.isDown));
    }

    // ───────────────────────── Fin du mini-jeu ─────────────────────────

    private void OnCrosswordCompleted()
    {
        crosswordFinalAnswer.CreateAnswer(crosswordData);
        OnGridCompleted?.Invoke();
    }

    private void CompleteGame()
    {
        isCompleted = true;
        TriggerGameCompleted();
    }

    // ───────────────────────── Sauvegarde ─────────────────────────

    private void InitializeCrosswordState()
    {
        CrosswordState state = ServiceLocator.Get<WorldState>().crosswordState;

        if (!state.initialized)
            CreateCrosswordState(state);
        else
            RestoreCrosswordState(state);
    }

    private void CreateCrosswordState(CrosswordState state)
    {
        state.initialized = true;
    }

    private void RestoreCrosswordState(CrosswordState state)
    {
        if (state.crosswordSolved)
        {
            crosswordView.Solve();
            crosswordFinalAnswer.CreateAnswer(crosswordData);
            crosswordFinalAnswer.RestoreState(state);
            return;
        }

        crosswordView.RestoreState(state);
    }

    private void SaveCrosswordState(CrosswordState state)
    {
        crosswordView.SaveState(state);
        state.crosswordSolved = crosswordView.solved;
        state.finalAnswer = crosswordFinalAnswer.GetAnswer();
    }
}

[Serializable]
public class CrosswordState
{
    public bool initialized;
    public bool crosswordSolved;
    public string finalAnswer = string.Empty;
    public List<CrosswordTileState> tiles = new();

    [Serializable]
    public class CrosswordTileState
    {
        public Vector2Int position;
        public char letter;
        public bool locked;
    }
}