using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CrosswordData", menuName = "Crossword/Data")]
public class CrosswordData : ScriptableObject
{
    public List<WordData> words;
    public string finalWord;
    public List<Vector2Int> finalWordLetterPositions;
}

[Serializable]
public class WordData
{
    public string word;

    [TextArea]
    public string hint;

    public Vector2Int originPosition;
    public bool isDown;

    [NonSerialized] public int Number;
}