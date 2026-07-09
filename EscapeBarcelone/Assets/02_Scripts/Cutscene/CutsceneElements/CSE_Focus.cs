using UnityEngine;

public class CSE_Focus : CutsceneElementBase
{
    [SerializeField] private bool show;
    [SerializeField] private Vector2 size;
    [SerializeField] private Vector2 offset;

    [SerializeField] private int orderInLayer = 0;

    public override void Execute()
    {
        base.Execute();
        FocusPanelTutorial focusPanel = cutsceneHandler.focusPanel;

        if (show)
        {
            focusPanel.Show();
            focusPanel.SetMaterialOffset(offset);
            focusPanel.SetMaterialSize(size);
            focusPanel.SetOrderInLayer(orderInLayer);
        }
        else
        {
            focusPanel.Hide();
        }

        StartCoroutine(WaitAndAdvance());
    }
}
