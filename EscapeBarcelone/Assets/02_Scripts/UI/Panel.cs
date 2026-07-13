using System;
using UnityEngine;

public abstract class Panel : MonoBehaviour
{
    protected CanvasGroup canvasGroup;

    private void Awake()
    {
        Initialize();
    }

    protected virtual void Initialize()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public virtual void Show()
    {
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1.0f;
    }

    public virtual void Hide()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.0f;
    }
}