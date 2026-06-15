using System;
using UnityEngine;

[CreateAssetMenu(fileName = "TextSequence", menuName = "Scriptable Objects/Text Sequence")]
public class TextSequence : ScriptableObject
{
    public TextData[] texts;
}

[Serializable]
public class TextData
{
    [TextArea]
    public string text;

    public bool autoAdvance;
    public float duration = 2f;
    public bool inputField;
}