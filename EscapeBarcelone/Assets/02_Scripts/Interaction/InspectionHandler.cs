using System;
using System.Collections.Generic;
using UnityEngine;

public class InspectionHandler : MonoBehaviour
{
    [SerializeField] private GameObject darkBackground;
    [SerializeField] private GameObject closeButton;
    [SerializeField] private SpriteRenderer darkBackgroundRenderer;

    private const int BaseSortingOrder = 0;
    private const int SortingStep = 10;

    private Stack<GameObject> inspectionStack = new();

    private bool canClose = true;

    public event Action OnInspectionClosed;

    private void Awake()
    {
        ServiceLocator.Register<InspectionHandler>(this);
    }

    public void Inspect(GameObject detailedView)
    {
        if (inspectionStack.Count == 0)
        {
            darkBackground.SetActive(true);
            closeButton.SetActive(canClose);
        }

        inspectionStack.Push(detailedView);
        detailedView.SetActive(true);

        ServiceLocator.Get<InteractionManager>()
            .SetCurrentInspection(detailedView);

        darkBackgroundRenderer.sortingOrder =
            BaseSortingOrder + inspectionStack.Count * SortingStep;
    }

    public void Close()
    {
        if (inspectionStack.Count == 0)
            return;

        GameObject currentView = inspectionStack.Pop();
        currentView.SetActive(false);

        OnInspectionClosed?.Invoke();

        if (inspectionStack.Count == 0)
        {
            darkBackground.SetActive(false);
            closeButton.SetActive(false);

            ServiceLocator.Get<InteractionManager>()
                .ClearCurrentInspection();

            canClose = true;

            return;
        }

        ServiceLocator.Get<InteractionManager>()
            .SetCurrentInspection(inspectionStack.Peek());

        darkBackgroundRenderer.sortingOrder =
            BaseSortingOrder + inspectionStack.Count * SortingStep;
    }

    public void SetCanClose(bool canClose)
    {
        this.canClose = canClose;

        if (inspectionStack.Count > 0)
            closeButton.SetActive(canClose);
    }
}