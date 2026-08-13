using System.Collections.Generic;
using System.Linq;

public class InteractionStack
{
    private Stack<InteractionElementBase> interactionStack = new();
    private GameplayPanel gameplayPanel;

    public InteractionStack()
    {
        //gameplayPanel = UIManager.Instance.gameplayPanel;
        gameplayPanel.OnCloseButtonClicked += CloseCurrentInteraction;
    }

    public int Count => interactionStack.Count;

    public void AddInteractionElement(InteractionElementBase element)
    {
        if (interactionStack.Contains(element))
            return;

        interactionStack.Push(element);
        UpdateCloseButton();
    }

    public void CloseCurrentInteraction()
    {
        if (interactionStack.Count == 0)
            return;

        InteractionElementBase element = interactionStack.Pop();
        element.Close();

        UpdateCloseButton();
    }

    public void RemoveInteractionElement(InteractionElementBase element)
    {
        if (!interactionStack.Contains(element))
            return;

        interactionStack = new Stack<InteractionElementBase>(
            interactionStack.Where(x => x != element)
        );

        UpdateCloseButton();
    }

    private void UpdateCloseButton()
    {
        gameplayPanel.SetCloseButtonVisible(interactionStack.Count > 0);
    }
}