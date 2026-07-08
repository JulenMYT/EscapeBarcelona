using UnityEngine;

public class CSE_Focus : CutsceneElementBase
{
    [SerializeField] private bool show;
    [SerializeField] private Vector2 size;
    [SerializeField] private Vector2 offset;

    public override void Execute()
    {
        base.Execute();
        FocusPanel focusPanel = cutsceneHandler.focusPanel;

        if (show)
        {
            focusPanel.Show();
            focusPanel.SetMaterialOffset(offset);
            focusPanel.SetMaterialSize(size);
            focusPanel.Show();
        }
        else
        {
            focusPanel.Hide();
        }

        StartCoroutine(WaitAndAdvance());
    }
}
