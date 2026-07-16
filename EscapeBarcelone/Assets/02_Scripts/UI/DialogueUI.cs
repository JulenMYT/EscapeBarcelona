using System.Security;
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

    public void DisplayLine(string speakerName, string lineText)
    {
        this.speakerName.text = speakerName;
        dialogueText.text = lineText;
    }

    public override void Show()
    {
        canvasGroup.alpha = 1.0f;
    }
}
