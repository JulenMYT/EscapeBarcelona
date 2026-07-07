using TMPro;
using UnityEngine;

public class DialogueUI : Panel
{
    [SerializeField] private TMP_Text speakerName;
    [SerializeField] private TMP_Text dialogueText;

    protected override void Initialize()
    {
        base.Initialize();
        Hide();
    }

    public void DisplayLine(Dialogue.DialogueLine line)
    {
        speakerName.text = line.speakerName;
        dialogueText.text = line.lineText;
    }
}
