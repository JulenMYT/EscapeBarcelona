using UnityEngine;

public class CSE_WaitForClick : CutsceneElementBase
{
    [SerializeField] private InteractionElementBase interactionElement;
    [SerializeField] private bool setInteractableAfterClick = true;

    public override void Execute()
    {
        interactionElement.OnInteract += OnInteraction;
        interactionElement.SetInteractable(true);
    }

    private void OnInteraction()
    {
        interactionElement.OnInteract -= OnInteraction;
        if (!setInteractableAfterClick)
            interactionElement.SetInteractable(false);
        cutsceneHandler.PlayNextElement();
    }
}
