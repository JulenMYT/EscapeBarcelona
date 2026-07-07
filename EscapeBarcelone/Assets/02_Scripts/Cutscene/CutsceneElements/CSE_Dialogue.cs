using System;
using UnityEngine;

public class CSE_Dialogue : CutsceneElementBase
{
    [SerializeField] private Dialogue dialogue;

    public override void Execute()
    {
        cutsceneHandler.dialogueHandler.StartDialogue(dialogue);
        cutsceneHandler.dialogueHandler.OnDialogueComplete += OnDialogueComplete;
    }

    private void OnDialogueComplete()
    {
        cutsceneHandler.dialogueHandler.OnDialogueComplete -= OnDialogueComplete;
        cutsceneHandler.PlayNextElement();
    }
}
