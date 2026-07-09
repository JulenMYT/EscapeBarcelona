using UnityEngine;

public class CSE_Unhighlight : CutsceneElementBase
{
    public override void Execute()
    {
        base.Execute();

        cutsceneHandler.focusPanel.Unhighlight();
        cutsceneHandler.focusPanel.Hide();

        cutsceneHandler.PlayNextElement();
    }
}