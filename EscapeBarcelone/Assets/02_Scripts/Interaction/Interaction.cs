using System;
using UnityEngine;

public class Interaction : MonoBehaviour
{
    public Action OnClick;

    [SerializeField] private int highlightSortingOrder = 101;

    private SpriteRenderer spriteRenderer;
    private int defaultSortingOrder;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        defaultSortingOrder = spriteRenderer.sortingOrder;
    }

    public void Highlight()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.sortingOrder = highlightSortingOrder;
    }

    public void RemoveHighlight()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.sortingOrder = defaultSortingOrder;
    }

    protected virtual void OnMouseDown()
    {
        if (!ServiceLocator.Get<InteractionManager>().CanInteract(gameObject))
            return;

        OnClick?.Invoke();
        Interact();
    }

    protected virtual void Interact()
    {
    }
}