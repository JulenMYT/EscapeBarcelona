using UnityEngine;

public class CSE_WaitForClose : CutsceneElementBase
{
    [SerializeField] private InteractionElementBase interactionElement;

    public override void Execute()
    {
        interactionElement.OnClose += OnClose;
        interactionElement.SetInteractable(true);
    }

    private void OnClose()
    {
        interactionElement.OnClose -= OnClose;
        cutsceneHandler.PlayNextElement();
    }
}
