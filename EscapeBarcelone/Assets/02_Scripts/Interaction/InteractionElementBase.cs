using System;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class InteractionElementBase : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Color highlightColor = Color.orange;
    private SpriteRenderer spriteRenderer;

    public event Action OnInteract;

    private void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public virtual void Interact()
    {
        OnInteract?.Invoke();
    }

    public virtual void Enter() 
    {
        if (spriteRenderer)
            spriteRenderer.color = highlightColor;
    }

    public virtual void Exit() 
    {
        if (spriteRenderer)
            spriteRenderer.color = Color.white;
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
