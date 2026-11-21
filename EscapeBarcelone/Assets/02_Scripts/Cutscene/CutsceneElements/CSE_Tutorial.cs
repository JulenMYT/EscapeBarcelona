using UnityEngine;

public class CSE_Tutorial : CutsceneElementBase
{
    [SerializeField] protected Tutorial tutorial;

    public override void Execute()
    {
        // Callback propre à CE tuto : même s'il doit attendre la fin d'un autre,
        // on ne sera prévenu qu'à sa propre fin.
        cutsceneHandler.tutorialHandler.StartTutorial(tutorial, OnTutorialComplete);
    }

    protected virtual void OnTutorialComplete()
    {
        cutsceneHandler.PlayNextElement();
    }
}
