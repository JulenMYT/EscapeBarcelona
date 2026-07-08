using System;
using UnityEngine;
using UnityEngine.EventSystems;

public enum InteractionType
{
    OutlineHighlight,
    NoneOutline,
    Stretch,
    None
}

public abstract class InteractionElementBase : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Color highlightColor = Color.orange;
    [SerializeField] private InteractionType interactionType = InteractionType.NoneOutline;
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

        switch (interactionType)
        {
            case InteractionType.OutlineHighlight:
                if (spriteRenderer)
                    spriteRenderer.material = outlineMaterial; 
                break;
            case InteractionType.NoneOutline:
                if (spriteRenderer)
                    spriteRenderer.material = baseMaterial; 
                break;
            case InteractionType.Stretch:

                break;
            default:
                break;
        }
    }

    public virtual void Interact()
    {
        OnInteract?.Invoke();
    }

    public virtual void Enter() 
    {
        switch (interactionType)
        {
            case InteractionType.OutlineHighlight:
                if (spriteRenderer)
                    spriteRenderer.color = highlightColor; 
                break;
            case InteractionType.NoneOutline:
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
        switch (interactionType)
        {
            case InteractionType.OutlineHighlight:
                if (spriteRenderer)
                    spriteRenderer.color = Color.white;
                break;
            case InteractionType.NoneOutline:
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
