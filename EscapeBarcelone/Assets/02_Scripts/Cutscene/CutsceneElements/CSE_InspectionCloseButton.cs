using UnityEngine;

public class CSE_InspectionCloseButton : CutsceneElementBase
{
    [SerializeField] private bool canClose;

    public override void Execute()
    {
        ServiceLocator.Get<InspectionHandler>()
            .SetCanClose(canClose);

        cutsceneHandler.PlayNextElement();
    }
}