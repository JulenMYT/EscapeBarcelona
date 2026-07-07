using System;
using UnityEngine;

public abstract class Panel : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    private void Start()
    {
        Initialize();
    }

    protected virtual void Initialize()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Show()
    {
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1.0f;
    }

    public void Hide()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.0f;
    }
}