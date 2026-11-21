using System;
using UnityEngine;

public class Book : MonoBehaviour
{
    [SerializeField] private GameObject bookUI;
    [SerializeField] private GameObject openButton;
    [SerializeField] private GameObject closeButton;

    private bool isOpen = false;

    public bool IsOpen => isOpen;

    public event Action OnBookOpened;
    public event Action OnBookClosed;

    private void Awake()
    {
        ServiceLocator.Register<Book>(this);

        // Le carnet démarre toujours fermé, même s'il est resté actif dans la scène.
        // Le bouton d'ouverture n'est pas touché : c'est le HUD qui le débloque.
        bookUI.SetActive(false);
        closeButton.SetActive(false);
    }

    public void OpenBook()
    {
        if (isOpen) return;

        // Un seul panneau à la fois.
        ServiceLocator.Get<Compass>()?.CloseCompass();

        isOpen = true;

        bookUI.SetActive(true);
        openButton.SetActive(false);
        closeButton.SetActive(true);
        ServiceLocator.Get<InteractionManager>()?.SetPanelOpen(this, true);
        OnBookOpened?.Invoke();
    }

    public void CloseBook()
    {
        if (!isOpen) return;

        isOpen = false;

        bookUI.SetActive(false);
        openButton.SetActive(true);
        closeButton.SetActive(false);
        ServiceLocator.Get<InteractionManager>()?.SetPanelOpen(this, false);
        OnBookClosed?.Invoke();
    }
}
