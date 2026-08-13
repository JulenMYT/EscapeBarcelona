using UnityEngine;

public class CSE_Tutorial : CutsceneElementBase
{
    [SerializeField] protected Tutorial tutorial;

    public override void Execute()
    {
        cutsceneHandler.tutorialHandler.OnSequenceComplete += OnTutorialComplete;
        cutsceneHandler.tutorialHandler.StartTutorial(tutorial);
    }

    protected virtual void OnTutorialComplete()
    {
        cutsceneHandler.tutorialHandler.OnSequenceComplete -= OnTutorialComplete;
        cutsceneHandler.PlayNextElement();
    }
}