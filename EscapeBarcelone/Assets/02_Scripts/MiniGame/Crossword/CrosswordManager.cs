using System;
using System.Collections.Generic;
using UnityEngine;

public class CrosswordManager : MiniGame
{
    [Header("References")]
    [SerializeField] private CrosswordData crosswordData;
    [SerializeField] private CrosswordView crosswordView;
    [SerializeField] private CrosswordDefinition crosswordDefinition;
    [SerializeField] private CrosswordFinalAnswer crosswordFinalAnswer;

    private CrosswordGridCreator crosswordGridCreator;
    private bool isCompleted;

    private void Awake()
    {
        crosswordGridCreator = new CrosswordGridCreator();
    }

    protected override void Initialize()
    {
        crosswordView.OnCrosswordCompleted += OnCrosswordCompleted;
        crosswordFinalAnswer.OnAnswerCompleted += CompleteGame;

        CreateCrossword();
        InitializeCrosswordState();
    }

    private void OnDisable()
    {
        if (isCompleted)
            return;

        SaveCrosswordState(ServiceLocator.Get<WorldState>().crosswordState);
    }

    private void OnDestroy()
    {
        crosswordView.OnCrosswordCompleted -= OnCrosswordCompleted;
        crosswordFinalAnswer.OnAnswerCompleted -= CompleteGame;
    }

    private void CreateCrossword()
    {
        crosswordGridCreator.CreateTiles(crosswordData);
        crosswordDefinition.CreateDefinitions(crosswordData);
        crosswordView.GenerateGrid(crosswordGridCreator);
    }

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

    private void OnCrosswordCompleted()
    {
        crosswordFinalAnswer.CreateAnswer(crosswordData);
    }

    private void CompleteGame()
    {
        isCompleted = true;
        TriggerGameCompleted();
    }

    protected override void SolveMiniGame()
    {
        CreateCrossword();

        crosswordView.Solve();
        crosswordFinalAnswer.CreateAnswer(crosswordData);
        crosswordFinalAnswer.Solve();

        isCompleted = true;
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