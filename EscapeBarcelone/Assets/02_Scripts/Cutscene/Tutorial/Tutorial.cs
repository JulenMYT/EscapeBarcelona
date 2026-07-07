using UnityEngine;

[CreateAssetMenu(fileName = "Tutorial", menuName = "Scriptable Objects/Tutorial")]
public class Tutorial : ScriptableObject
{
    [TextArea(3, 10)]
    public string[] tutorialLines;
}
