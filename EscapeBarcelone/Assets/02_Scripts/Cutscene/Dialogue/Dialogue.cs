using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue", menuName = "Scriptable Objects/Dialogue")]
public class Dialogue : ScriptableObject
{
    public DialogueLine[] dialogueLines;

    [System.Serializable]
    public class DialogueLine
    {
        public string speakerName;
        public string lineText;
        public AudioClip voiceClip;
    }
}
