using System;
using UnityEngine;

public class CSE_Tutorial : CutsceneElementBase
{
    [SerializeField] private Tutorial tutorial;

    public override void Execute()
    {
        cutsceneHandler.tutorialHandler.StartTutorial(tutorial);
        cutsceneHandler.tutorialHandler.OnTutorialComplete += OnTutorialComplete;
    }

    private void OnTutorialComplete()
    {
        cutsceneHandler.tutorialHandler.OnTutorialComplete -= OnTutorialComplete;
        cutsceneHandler.PlayNextElement();
    }
}
