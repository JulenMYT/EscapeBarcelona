using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickableUI : MonoBehaviour, IPointerClickHandler
{
    public event Action OnClicked;

    [SerializeField] private bool isEnabled = true;

    public void EnableClick()
    {
        isEnabled = true;
    }

    public void DisableClick()
    {
        isEnabled = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isEnabled)
        {
            return;
        }

        OnClicked?.Invoke();
    }
}