using UnityEngine;

public class CrosswordManager : MiniGame
{
    [SerializeField] private CrosswordData crosswordData;
    [SerializeField] private CrosswordView crosswordView;
    [SerializeField] private CrosswordDefinition crosswordDefinition;

    private CrosswordGridCreator crosswordGridCreator;
    private int completedWords;

    private void Awake()
    {
        crosswordGridCreator = new CrosswordGridCreator();
    }

    protected override void Initialize()
    {
        CreateCrossword();
        crosswordView.OnCrosswordCompleted += TriggerGameCompleted;
    }

    private void OnDestroy()
    {
        crosswordView.OnCrosswordCompleted -= TriggerGameCompleted;
    }

    public void CreateCrossword()
    {
        crosswordGridCreator.CreateTiles(crosswordData);
        crosswordDefinition.CreateDefinitions(crosswordData);
        crosswordView.GenerateGrid(crosswordGridCreator);
    }
}