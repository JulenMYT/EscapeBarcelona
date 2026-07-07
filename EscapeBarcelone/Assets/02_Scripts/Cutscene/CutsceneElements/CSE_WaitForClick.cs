using UnityEngine;

public class CSE_WaitForClick : CutsceneElementBase
{
    [SerializeField] private InteractionElementBase interactionElement;

    public override void Execute()
    {
        interactionElement.OnInteract += OnInteraction;
    }

    private void OnInteraction()
    {
        interactionElement.OnInteract -= OnInteraction;
        cutsceneHandler.PlayNextElement();
    }
}
