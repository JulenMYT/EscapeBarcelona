using UnityEngine;

public class CrosswordManager : MonoBehaviour
{
    [SerializeField] private CrosswordData crosswordData;
    [SerializeField] private CrosswordView crosswordView;

    private CrosswordGridCreator crosswordGridCreator;

    private void Awake()
    {
        crosswordGridCreator = new CrosswordGridCreator();
    }

    private void Start()
    {
        CreateCrossword();
    }

    public void CreateCrossword()
    {
        crosswordGridCreator.CreateTiles(crosswordData);
        crosswordView.GenerateGrid(crosswordGridCreator);
    }
}