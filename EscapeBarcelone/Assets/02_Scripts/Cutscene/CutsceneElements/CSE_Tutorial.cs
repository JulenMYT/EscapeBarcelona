using UnityEngine;

public class CSE_Tutorial : CutsceneElementBase
{
    [SerializeField] private Tutorial tutorial;

    public override void Execute()
    {
        cutsceneHandler.tutorialHandler.StartTutorial(tutorial);

        if (tutorial.autoHide)
        {
            cutsceneHandler.tutorialHandler.OnTutorialComplete += OnTutorialComplete;
        }
        else
        {
            cutsceneHandler.PlayNextElement();
        }
    }

    private void OnTutorialComplete()
    {
        cutsceneHandler.tutorialHandler.OnTutorialComplete -= OnTutorialComplete;
        cutsceneHandler.PlayNextElement();
    }
}