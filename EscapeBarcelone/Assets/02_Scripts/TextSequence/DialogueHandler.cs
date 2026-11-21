using System;
using UnityEngine;

public class DialogueHandler : TextSequenceHandler<Dialogue>
{
    [SerializeField] private DialogueUI dialogueUI;

    private void Awake()
    {
        ServiceLocator.Register<DialogueHandler>(this);
    }

    /// <param name="onComplete">Appelé à la fin de ce dialogue précisément.</param>
    public void StartDialogue(Dialogue dialogue, Action onComplete = null)
    {
        StartSequence(dialogue, onComplete);
    }

    protected override int GetLineCount()
    {
        return currentSequence.dialogueLines.Length;
    }

    protected override void DisplayCurrentLine()
    {
        Dialogue.DialogueLine line = currentSequence.dialogueLines[index];
        dialogueUI.DisplayLine(line.speakerName, line.lineText);
    }

    protected override bool IsLineAnimating() => dialogueUI.IsTyping;

    protected override void CompleteLineAnimation() => dialogueUI.CompleteLine();

    protected override void ShowUI()
    {
        dialogueUI.Show();
    }

    protected override void OnEndSequence()
    {
        dialogueUI.Hide();
    }
}
