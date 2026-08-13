using System;
using UnityEngine;

public class Interaction : MonoBehaviour
{
    public Action OnClick;

    [SerializeField] private int highlightSortingOrder = 101;

    private SpriteRenderer spriteRenderer;
    private int defaultSortingOrder;

    private static Material outlineMaterial;
    private Material defaultMaterial;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null )
        {
            defaultSortingOrder = spriteRenderer.sortingOrder;
            defaultMaterial = spriteRenderer.sharedMaterial;

            if (outlineMaterial == null)
                outlineMaterial = Resources.Load<Material>("Materials/Outline");
        }

        Initialize();
    }

    protected virtual void Initialize()
    {
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

    private void OnMouseEnter()
    {
        if (!ServiceLocator.Get<InteractionManager>().CanInteract(gameObject))
            return;

        spriteRenderer.sharedMaterial = outlineMaterial;
    }

    private void OnMouseExit()
    {
        spriteRenderer.sharedMaterial = defaultMaterial;
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