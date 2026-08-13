using System;
using System.Collections.Generic;
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
    [Header("Interaction")]
    [SerializeField] private bool isInteractable = true;
    [SerializeField] protected bool canBeClosed = true;
    [SerializeField] private Color highlightColor = Color.orange;

    [SerializeField] private InteractionType interactionType = InteractionType.None;
    [SerializeField] private InteractionType interactionTypeHover = InteractionType.Outline;

    private bool ignoreInteractionBlock;

    [Header("References")]
    protected SpriteRenderer spriteRenderer;

    public event Action OnInteract;
    public event Action OnClose;

    private const string outlineMaterialPath = "OutlineMaterial";
    private const string baseMaterialPath = "BaseMaterial";

    private static Material outlineMaterial;
    private static Material baseMaterial;

    protected static readonly Stack<Transform> focusStack = new();
    protected static InteractionStack interactionStack;

    public static Transform FocusRoot => focusStack.Count > 0 ? focusStack.Peek() : null;

    public bool IsInteractable
    {
        get => isInteractable;
        private set
        {
            isInteractable = value;

            if (!isInteractable)
                Exit();
        }
    }

    public bool IgnoreInteractionBlock
    {
        get => ignoreInteractionBlock;
        private set
        {
            ignoreInteractionBlock = value;

            if (!ignoreInteractionBlock)
                Exit();
        }
    }

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
        if (interactionStack == null)
            interactionStack = new InteractionStack();

        Initialize();
    }

    public virtual void Initialize()
    {

    }

    public void SetIgnoreInteractionBlock(bool value)
    {
        IgnoreInteractionBlock = value;
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
                    transform.localScale *= 1.2f;
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
                    transform.localScale = Vector3.one;
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
        }
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (ServiceLocator.Get<InteractionManager>().IsLocked && !IgnoreInteractionBlock)
            return;

        if (!IsInteractable)
            return;

        if (FocusRoot != null && !transform.IsChildOf(FocusRoot))
            return;

        Interact();
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (ServiceLocator.Get<InteractionManager>().IsLocked && !IgnoreInteractionBlock)
            return;

        if (!IsInteractable)
            return;

        if (FocusRoot != null && !transform.IsChildOf(FocusRoot))
            return;

        Enter();
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (ServiceLocator.Get<InteractionManager>().IsLocked && !IgnoreInteractionBlock)
            return;

        if (!IsInteractable)
            return;

        if (FocusRoot != null && !transform.IsChildOf(FocusRoot))
            return;

        Exit();
    }

    public virtual void Open()
    {
        gameObject.SetActive(true);

        if (canBeClosed)
            interactionStack.AddInteractionElement(this);
    }

    public virtual void Close()
    {
        gameObject.SetActive(false);

        if (canBeClosed)
            interactionStack.RemoveInteractionElement(this);

        OnClose?.Invoke();
    }

    public void SetInteractable(bool value)
    {
        IsInteractable = value;
    }

    public void SetClosable(bool value)
    {
        canBeClosed = value;
    }
}