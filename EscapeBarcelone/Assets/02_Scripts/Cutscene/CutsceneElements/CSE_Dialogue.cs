using UnityEngine;

public class CSE_Dialogue : CutsceneElementBase
{
    [SerializeField] private Dialogue dialogue;

    public override void Execute()
    {
        // Callback propre à CE dialogue (fonctionne aussi pour un dialogue vide,
        // ou s'il doit attendre la fin d'un autre dialogue).
        cutsceneHandler.dialogueHandler.StartDialogue(dialogue, OnDialogueComplete);
    }

    private void OnDialogueComplete()
    {
        cutsceneHandler.PlayNextElement();
    }
}
