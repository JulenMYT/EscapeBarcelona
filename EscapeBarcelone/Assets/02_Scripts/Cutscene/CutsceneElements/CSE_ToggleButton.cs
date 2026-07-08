using UnityEngine;
using UnityEngine.UI;

public class CSE_ToggleButton : CutsceneElementBase
{
    [SerializeField] private Button button;
    [SerializeField] private bool activate;

    public override void Execute()
    {
        button.interactable = activate;
        cutsceneHandler.PlayNextElement();
    }
}
