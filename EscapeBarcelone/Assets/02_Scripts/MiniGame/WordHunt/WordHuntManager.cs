using System.Linq;
using UnityEngine;

public class WordHuntManager : MonoBehaviour
{
    [SerializeField] private WordHuntData data;
    [SerializeField] private WordHuntView view;

    private void OnEnable()
    {
        view.SelectionEnded += HandleSelectionEnded;
    }

    private void OnDisable()
    {
        view.SelectionEnded -= HandleSelectionEnded;
    }

    private void Start()
    {
        view.CreateGrid(data);
    }

    private void HandleSelectionEnded(Vector2Int start, Vector2Int end, Vector2Int direction)
    {
        string word = string.Empty;
        Vector2Int position = start;

        while (position != end)
        {
            word += data.Rows[position.y][position.x];
            position += direction;
        }

        word += data.Rows[position.y][position.x];

        Debug.Log(word);
        if (data.Words.Contains(word))
            Debug.Log("Word found: " + word);
    }
}