using System;
using UnityEngine;

public class DialogueHandler : MonoBehaviour
{
    private int index = -1;
    private Dialogue currentDialogue;
    private DialogueUI dialogueUI;

    public event Action OnDialogueComplete;

    private void Start()
    {
        dialogueUI = FindAnyObjectByType<DialogueUI>();

        if (!dialogueUI)
        {
            Debug.LogError("DialogueUI not found in the scene.");
        }
    }

    private void Update()
    {
        if (currentDialogue != null && Input.GetKeyDown(KeyCode.Mouse0))
        {
            PlayNextLine();
        }
    }

    public void StartDialogue(Dialogue dialogue)
    {
        currentDialogue = dialogue;
        index = -1;
        dialogueUI.Show();
        PlayNextLine();
    }

    private void PlayCurrentLine()
    {
        if (index >= 0 && index < currentDialogue.dialogueLines.Length)
        {
            dialogueUI.DisplayLine(currentDialogue.dialogueLines[index]);
        }

        if (index >= currentDialogue.dialogueLines.Length)
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        currentDialogue = null;
        dialogueUI.Hide();
        OnDialogueComplete?.Invoke();
    }

    public void PlayNextLine()
    {
        index++;
        PlayCurrentLine();
    }
}
