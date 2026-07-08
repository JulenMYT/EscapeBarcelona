using UnityEngine;

public class CSE_NewCutscene : CutsceneElementBase
{
    [SerializeField] private CutsceneInitiator cutsceneInitiator;

    public override void Execute()
    {
        cutsceneInitiator.StartCutscene();
        cutsceneHandler.PlayNextElement();
    }
}
