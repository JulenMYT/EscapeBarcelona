using UnityEngine;

public abstract class Panel : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

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