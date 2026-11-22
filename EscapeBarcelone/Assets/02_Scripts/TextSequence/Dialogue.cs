using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue", menuName = "Scriptable Objects/Dialogue")]
public class Dialogue : ScriptableObject
{
    [System.Serializable]
    public class DialogueLine
    {
        public string speakerName;
        [TextArea(3, 10)]
        public string lineText;
        public AudioClip voiceClip;
    }

    public DialogueLine[] dialogueLines;
}
