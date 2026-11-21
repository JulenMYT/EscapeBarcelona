using System;
using UnityEngine;

public class Compass : MonoBehaviour
{
    [SerializeField] private GameObject compassUI;
    [SerializeField] private GameObject openButton;
    [SerializeField] private GameObject closeButton;

    private bool isOpen = false;

    public bool IsOpen => isOpen;

    public event Action OnCompassOpened;
    public event Action OnCompassClosed;

    private void Awake()
    {
        ServiceLocator.Register<Compass>(this);

        // La boussole démarre toujours fermée. Le bouton d'ouverture est géré par le HUD.
        compassUI.SetActive(false);
        closeButton.SetActive(false);
    }

    public void OpenCompass()
    {
        if (isOpen) return;

        // Un seul panneau à la fois.
        ServiceLocator.Get<Book>()?.CloseBook();

        isOpen = true;

        compassUI.SetActive(true);
        openButton.SetActive(false);
        closeButton.SetActive(true);
        ServiceLocator.Get<InteractionManager>()?.SetPanelOpen(this, true);
        OnCompassOpened?.Invoke();
    }

    public void CloseCompass()
    {
        if (!isOpen) return;

        isOpen = false;

        compassUI.SetActive(false);
        openButton.SetActive(true);
        closeButton.SetActive(false);
        ServiceLocator.Get<InteractionManager>()?.SetPanelOpen(this, false);
        OnCompassClosed?.Invoke();
    }
}
