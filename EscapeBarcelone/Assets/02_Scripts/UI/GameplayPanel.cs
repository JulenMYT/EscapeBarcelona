using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameplayPanel : Panel
{
    [SerializeField] private Button closeButton;

    [SerializeField] private Button validateButton;

    private Stack<InteractionElementBase> interactionStack = new();

    public event Action OnValidateButtonClicked;

    protected override void Initialize()
    {
        base.Initialize();
        closeButton.onClick.AddListener(CloseCurrentInteraction);
        validateButton.onClick.AddListener(() => OnValidateButtonClicked?.Invoke());
    }

    public void OpenInteractionElement(InteractionElementBase element)
    {
        if (interactionStack.Contains(element))
            return;

        interactionStack.Push(element);

        ShowCloseButton();
    }

    private void CloseCurrentInteraction()
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
        closeButton.gameObject.SetActive(interactionStack.Count > 0);
    }

    public void ShowCloseButton()
    {
        closeButton.gameObject.SetActive(true);
    }

    public void HideCloseButton()
    {
        closeButton.gameObject.SetActive(false);
    }

    public void ShowValidateButton()
    {
        validateButton.gameObject.SetActive(true);
    }

    public void HideValidateButton()
    {
        validateButton.gameObject.SetActive(false);
    }
}