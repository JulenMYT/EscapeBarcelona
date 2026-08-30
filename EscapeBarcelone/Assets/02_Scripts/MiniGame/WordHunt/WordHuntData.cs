using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WordHuntData", menuName = "MiniGames/WordHunt")]
public class WordHuntData : ScriptableObject
{
    [SerializeField] private List<string> rows;
    [SerializeField] private List<string> words;

    public IReadOnlyList<string> Rows => rows;
    public IReadOnlyList<string> Words => words;
}