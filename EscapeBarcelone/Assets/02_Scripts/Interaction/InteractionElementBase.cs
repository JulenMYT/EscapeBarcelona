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
    [SerializeField] private bool isInteractable = true;
    [SerializeField] private bool canBeClosed = true;

    [SerializeField] private Color highlightColor = Color.orange;
    [SerializeField] private InteractionType interactionType = InteractionType.None;
    [SerializeField] private InteractionType interactionTypeHover = InteractionType.Outline;
    protected SpriteRenderer spriteRenderer;

    public event Action OnInteract;
    public event Action OnClose;

    private const string outlineMaterialPath = "OutlineMaterial";
    private const string baseMaterialPath = "BaseMaterial";

    private static Material outlineMaterial;
    private static Material baseMaterial;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (outlineMaterial == null)
            outlineMaterial = Resources.Load<Material>(outlineMaterialPath);

        if (baseMaterial == null)
            baseMaterial = Resources.Load<Material>(baseMaterialPath);
    }

    protected virtual void Start()
    {
        Initialize();
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

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (!isInteractable)
            return;
        Interact();
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (!isInteractable)
            return;
        Enter();
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (!isInteractable)
            return;
        Exit();
    }

    public virtual void Initialize()
    {

    }

    public virtual void Open()
    {
        Debug.Log($"Opening {gameObject.name}");
        gameObject.SetActive(true);

        if (canBeClosed)
        {
            GameplayPanel gameplayPanel = FindAnyObjectByType<GameplayPanel>();
            gameplayPanel.OpenInteractionElement(this);
        }
    }

    public virtual void Close()
    {
        gameObject.SetActive(false);

        if (canBeClosed)
        {
            GameplayPanel gameplayPanel = FindAnyObjectByType<GameplayPanel>();
            gameplayPanel.RemoveInteractionElement(this);
        }

        OnClose?.Invoke();
    }

    public void SetInteractable(bool value)
    {
        isInteractable = value;
    }
}
