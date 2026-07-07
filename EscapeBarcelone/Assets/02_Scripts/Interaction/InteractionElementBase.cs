using UnityEngine;
using UnityEngine.EventSystems;

public class InteractionElementBase : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Color highlightColor = Color.orange;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public virtual void Interact()
    {
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
