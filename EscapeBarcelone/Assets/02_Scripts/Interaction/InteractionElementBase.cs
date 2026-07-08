using System;
using UnityEngine;
using UnityEngine.EventSystems;

public enum InteractionType
{
    Outline,
    Highlight,
    Stretch,
    None
}

public abstract class InteractionElementBase : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Color highlightColor = Color.orange;
    [SerializeField] private InteractionType interactionType = InteractionType.None;
    [SerializeField] private InteractionType interactionTypeHover = InteractionType.Outline;
    private SpriteRenderer spriteRenderer;

    public event Action OnInteract;

    private const string outlineMaterialPath = "OutlineMaterial";
    private const string baseMaterialPath = "BaseMaterial";

    private Material outlineMaterial;
    private Material baseMaterial;

    private void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        outlineMaterial = Resources.Load<Material>(outlineMaterialPath);
        baseMaterial = Resources.Load<Material>(baseMaterialPath);
    }

    public virtual void Interact()
    {
        OnInteract?.Invoke();
    }

    public virtual void Enter() 
    {
        switch (interactionTypeHover)
        {
            case InteractionType.Highlight:
                if (spriteRenderer)
                    spriteRenderer.color = highlightColor; 
                break;
            case InteractionType.Outline:
                if (spriteRenderer)
                    spriteRenderer.material = outlineMaterial; 
                break;
            case InteractionType.Stretch:
                if (spriteRenderer)
                    spriteRenderer.transform.localScale *= 1.2f;
                break;
            default:
                break;
        }
    }

    public virtual void Exit() 
    {
        switch (interactionTypeHover)
        {
            case InteractionType.Highlight:
                if (spriteRenderer)
                    spriteRenderer.color = Color.white;
                break;
            case InteractionType.Outline:
                if (spriteRenderer)
                    spriteRenderer.material = baseMaterial;
                break;
            case InteractionType.Stretch:
                if (spriteRenderer)
                    spriteRenderer.transform.localScale /= 1.2f;
                break;
            default:
                break;
        }

        switch (interactionType)
        {
            case InteractionType.Highlight:
                if (spriteRenderer)
                    spriteRenderer.color = highlightColor;
                break;
            case InteractionType.Outline:
                if (spriteRenderer)
                    spriteRenderer.material = outlineMaterial;
                break;
            default:
                break;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Interact();
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        Enter();
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        Exit();
    }
}
