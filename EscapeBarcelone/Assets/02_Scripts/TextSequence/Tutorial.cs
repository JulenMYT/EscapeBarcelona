using UnityEngine;

[System.Serializable]
public class TutorialLine
{
    [TextArea(3, 10)]
    public string text;

    public bool waitForInteraction;
}

[CreateAssetMenu(fileName = "Tutorial", menuName = "Scriptable Objects/Tutorial")]
public class Tutorial : ScriptableObject
{
    public TutorialLine[] tutorialLines;

    [Range(0, 1080)]
    public int textPos;
}