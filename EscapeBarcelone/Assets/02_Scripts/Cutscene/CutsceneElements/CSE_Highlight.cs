using System.Linq;
using UnityEngine;

public class CSE_Highlight : CutsceneElementBase
{
    [SerializeField] private GameObject[] objectsToHighlight;

    [Header("Focus")]
    [SerializeField] private Vector2 size;
    [SerializeField] private Vector2 offset;

    public override void Execute()
    {
        base.Execute();

        FocusPanelTutorial focusPanel = cutsceneHandler.focusPanel;

        focusPanel.Show();
        focusPanel.SetMaterialOffset(offset);
        focusPanel.SetMaterialSize(size);

        SpriteRenderer[] renderers = objectsToHighlight
            .SelectMany(x => x.GetComponentsInChildren<SpriteRenderer>())
            .ToArray();

        focusPanel.Highlight(renderers);

        cutsceneHandler.PlayNextElement();
    }
}