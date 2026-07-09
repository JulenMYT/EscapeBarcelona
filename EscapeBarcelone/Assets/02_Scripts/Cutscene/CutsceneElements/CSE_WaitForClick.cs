using UnityEngine;

public class CSE_WaitForClick : CutsceneElementBase
{
    [SerializeField] private InteractionElementBase interactionElement;

    public override void Execute()
    {
        interactionElement.OnInteract += OnInteraction;
        interactionElement.SetIgnoreInteractionBlock(true);
    }

    private void OnInteraction()
    {
        interactionElement.OnInteract -= OnInteraction;
        interactionElement.SetIgnoreInteractionBlock(false);
        cutsceneHandler.PlayNextElement();
    }
}
