using UnityEngine;

public class CSE_HidePanel : CutsceneElementBase
{
    [SerializeField] private Panel panel;

    public override void Execute()
    {
        base.Execute();

        panel.Hide();

        cutsceneHandler.PlayNextElement();
    }
}
