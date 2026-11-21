using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Objet cliquable du décor. Les clics et le survol passent par l'EventSystem
/// (Physics2DRaycaster sur la caméra), donc l'UI bloque automatiquement les clics.
/// </summary>
public class Interaction : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public event Action OnClick;

    [Tooltip("Flags requis / interdits pour que l'objet soit cliquable. Vide = toujours cliquable.")]
    [SerializeField] private StoryCondition condition = new();

    private SpriteRenderer spriteRenderer;

    private static Material outlineMaterial;
    private Material defaultMaterial;

    private string defaultSortingLayer;

    private bool isHovered;
    private bool isOutlined;

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

    protected virtual void OnEnable()
    {
        InteractionManager.StateChanged += RefreshHover;
    }

    protected virtual void OnDisable()
    {
        InteractionManager.StateChanged -= RefreshHover;

        isHovered = false;
        RefreshHover();
    }

    public bool CanInteract()
    {
        InteractionManager manager = ServiceLocator.Get<InteractionManager>();

        if (manager == null || !manager.CanInteract(gameObject))
            return false;

        return condition.IsMet();
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        RefreshHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        RefreshHover();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (!CanInteract())
            return;

        OnClick?.Invoke();
        Interact();

        // L'interaction peut avoir changé l'état (inspection ouverte, objet désactivé…).
        RefreshHover();
    }

    /// <summary>
    /// Met à jour le contour et le curseur selon le survol et l'état actuel
    /// (verrouillage, inspection, conditions). Appelé aussi quand l'état de
    /// l'InteractionManager change, pour ne pas attendre que la souris bouge.
    /// </summary>
    private void RefreshHover()
    {
        bool shouldOutline = isHovered && isActiveAndEnabled && CanInteract();

        if (shouldOutline == isOutlined)
            return;

        isOutlined = shouldOutline;

        if (spriteRenderer != null && outlineMaterial != null)
            spriteRenderer.sharedMaterial = shouldOutline ? outlineMaterial : defaultMaterial;

        ServiceLocator.Get<InteractionManager>()?.SetHovered(this, shouldOutline);
    }

    protected virtual void Interact()
    {
    }
}
