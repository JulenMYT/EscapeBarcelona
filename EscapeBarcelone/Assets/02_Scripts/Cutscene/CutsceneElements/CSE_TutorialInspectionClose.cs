using UnityEngine;

public class CSE_TutorialInspectionClose : CSE_Tutorial
{
    public override void Execute()
    {
        ServiceLocator.Get<InspectionHandler>()
            .OnInspectionClosed += OnInspectionClosed;

        base.Execute();
    }

    private void OnInspectionClosed()
    {
        ServiceLocator.Get<InspectionHandler>()
            .OnInspectionClosed -= OnInspectionClosed;

        cutsceneHandler.tutorialHandler.CompleteInteraction();
    }

    protected override void OnTutorialComplete()
    {
        ServiceLocator.Get<InspectionHandler>()
            .OnInspectionClosed -= OnInspectionClosed;

        base.OnTutorialComplete();
    }
}