using System;
using UnityEngine;
using UnityEngine.UI;

public class GameplayPanel : Panel
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Button validateButton;
    [SerializeField] private Button toggleButton;

    public event Action OnCloseButtonClicked;
    public event Action OnValidateButtonClicked;
    public event Action OnToggleButtonClicked;

    protected override void Initialize()
    {
        base.Initialize();

        closeButton.onClick.AddListener(() => OnCloseButtonClicked?.Invoke());
        validateButton.onClick.AddListener(() => OnValidateButtonClicked?.Invoke());
        toggleButton.onClick.AddListener(() => OnToggleButtonClicked?.Invoke());
    }

    public void SetCloseButtonVisible(bool visible)
    {
        closeButton.gameObject.SetActive(visible);
    }

    public void SetValidateButtonVisible(bool visible)
    {
        validateButton.gameObject.SetActive(visible);
    }

    public void SetToggleButtonVisible(bool visible)
    {
        toggleButton.gameObject.SetActive(visible);
    }
}