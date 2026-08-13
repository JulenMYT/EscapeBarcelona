using System;
using UnityEngine;

public class CSE_Dialogue : CutsceneElementBase
{
    [SerializeField] private Dialogue dialogue;

    public override void Execute()
    {
        cutsceneHandler.dialogueHandler.StartDialogue(dialogue);
        cutsceneHandler.dialogueHandler.OnSequenceComplete += OnDialogueComplete;
    }

    private void OnDialogueComplete()
    {
        cutsceneHandler.dialogueHandler.OnSequenceComplete -= OnDialogueComplete;
        cutsceneHandler.PlayNextElement();
    }
}
