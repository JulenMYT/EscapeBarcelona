using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameplayPanel : Panel
{
    [SerializeField] private Button closeButton;

    [SerializeField] private Button validateButton;

    [SerializeField] private Button toggleButton;

    private Stack<InteractionElementBase> interactionStack = new();

    public event Action OnValidateButtonClicked;
    public event Action OnToggleButtonClicked;

    protected override void Initialize()
    {
        base.Initialize();
        closeButton.onClick.AddListener(CloseCurrentInteraction);
        validateButton.onClick.AddListener(() => OnValidateButtonClicked?.Invoke());
        toggleButton.onClick.AddListener(() => OnToggleButtonClicked?.Invoke());
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

    public void ShowToggleButton()
    {
        toggleButton.gameObject.SetActive(true);
    }

    public void HideToggleButton()
    {
        toggleButton.gameObject.SetActive(false);
    }
}