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

    private static readonly Stack<Transform> focusStack = new();

    private bool ignoreInteractionBlock;

    public static Transform FocusRoot => focusStack.Count > 0 ? focusStack.Peek() : null;

    public bool IsInteractable
    {
        get => isInteractable;
        private set
        {
            isInteractable = value;

            if (!isInteractable)
            {
                Exit();
            }
        }
    }

    public bool IgnoreInteractionBlock
    {
        get => ignoreInteractionBlock;
        private set
        {
            ignoreInteractionBlock = value;

            if (!ignoreInteractionBlock)
            {
                Exit();
            }
        }
    }

    public bool CanBeClosed
    {
        get => canBeClosed;
        private set => canBeClosed = value;
    }

    public static void PushFocus(Transform target)
    {
        focusStack.Push(target);
    }

    public static void PopFocus()
    {
        if (focusStack.Count > 0)
        {
            focusStack.Pop();
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
        Initialize();
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
                    spriteRenderer.transform.localScale *= 1.2f;
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
        if (InteractionManager.Blocked && !IgnoreInteractionBlock)
            return;

        if (!IsInteractable)
            return;

        if (FocusRoot != null && !transform.IsChildOf(FocusRoot))
            return;

        Interact();
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (InteractionManager.Blocked && !IgnoreInteractionBlock)
            return;

        if (!IsInteractable)
            return;

        if (FocusRoot != null && !transform.IsChildOf(FocusRoot))
            return;

        Enter();
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (!IsInteractable)
            return;

        Exit();
    }

    public virtual void Initialize()
    {

    }

    public virtual void Open()
    {
        gameObject.SetActive(true);

        if (CanBeClosed)
        {
            GameplayPanel gameplayPanel = FindAnyObjectByType<GameplayPanel>();
            gameplayPanel.OpenInteractionElement(this);
        }
    }

    public virtual void Close()
    {
        gameObject.SetActive(false);

        if (CanBeClosed)
        {
            GameplayPanel gameplayPanel = FindAnyObjectByType<GameplayPanel>();
            gameplayPanel.RemoveInteractionElement(this);
        }

        OnClose?.Invoke();
    }

    public void SetInteractable(bool value)
    {
        IsInteractable = value;
    }

    public void SetClosable(bool value)
    {
        CanBeClosed = value;
    }
}