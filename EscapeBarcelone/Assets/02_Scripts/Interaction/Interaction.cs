using System;
using UnityEngine;

public class Interaction : MonoBehaviour
{
    public Action OnClick;

    private SpriteRenderer spriteRenderer;

    private static Material outlineMaterial;
    private Material defaultMaterial;

    private string defaultSortingLayer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            defaultSortingLayer = spriteRenderer.sortingLayerName;
            defaultMaterial = spriteRenderer.sharedMaterial;

            if (outlineMaterial == null)
                outlineMaterial = Resources.Load<Material>("Materials/Outline");
        }

        AwakeInitialize();
    }

    protected virtual void AwakeInitialize()
    {
    }

    public void Highlight()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.sortingLayerName = "Highlight";
    }

    public void RemoveHighlight()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.sortingLayerName = defaultSortingLayer;
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