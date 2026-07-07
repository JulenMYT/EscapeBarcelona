using System;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class InteractionElementBase : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Color highlightColor = Color.orange;
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
        if (spriteRenderer)
            spriteRenderer.material = outlineMaterial;
    }

    public virtual void Exit() 
    {
        if (spriteRenderer)
            spriteRenderer.material = baseMaterial;
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
