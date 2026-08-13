using UnityEngine;

public class DialogueHandler : TextSequenceHandler<Dialogue>
{
    [SerializeField] private DialogueUI dialogueUI;

    private void Awake()
    {
        ServiceLocator.Register<DialogueHandler>(this);
    }

    public void StartDialogue(Dialogue dialogue)
    {
        StartSequence(dialogue);
    }

    protected override int GetLineCount()
    {
        return currentSequence.dialogueLines.Length;
    }

    protected override void DisplayCurrentLine()
    {
        dialogueUI.DisplayLine(
            currentSequence.dialogueLines[index].speakerName,
            currentSequence.dialogueLines[index].lineText
        );
    }

    protected override void ShowUI()
    {
        dialogueUI.Show();
    }

    protected override void OnEndSequence()
    {
        dialogueUI.Hide();
    }
}