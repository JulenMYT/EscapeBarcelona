using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CrosswordData", menuName = "Crossword/Data")]
public class CrosswordData : ScriptableObject
{
    public List<WordData> words;
}

[Serializable]
public class WordData
{
    public string word;

    [TextArea]
    public string hint;

    public Vector2Int originPosition;

    public bool isDown;
}