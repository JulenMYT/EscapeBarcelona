using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Fait défiler la grille des mots croisés quand elle est plus grande que son cadre
/// (le masque « GridMask »). Ajouté automatiquement par CrosswordView, rien à régler.
///
/// - Molette : haut / bas (Maj + molette : gauche / droite).
/// - Clic maintenu + glisser n'importe où dans le cadre : déplace la grille.
/// - La case sélectionnée au clavier reste toujours visible.
///
/// On lit la souris directement (et pas via un ScrollRect) parce que les cases sont des
/// champs de saisie qui « mangent » le glisser et la molette.
/// </summary>
public class CrosswordGridScroller : MonoBehaviour
{
    private const float WheelStep = 80f;
    private const float DragThreshold = 8f;
    private const float VisibleMargin = 10f;

    private RectTransform viewport;
    private RectTransform content;
    private Vector2 originalSizeDelta;
    private bool initialized;

    private bool pressedInside;
    private bool dragging;
    private Vector2 pressScreenPosition;
    private Vector2 lastLocalPoint;

    /// <summary>Taille disponible pour la grille quand elle tient dans le cadre (cadre moins sa marge).</summary>
    public Vector2 AvailableSize => viewport.rect.size + originalSizeDelta;

    private void Update()
    {
        if (!initialized || CutsceneDirector.IsPlaying)
            return;

        Mouse mouse = Mouse.current;

        if (mouse == null)
            return;

        Vector2 screenPosition = mouse.position.ReadValue();
        Camera eventCamera = EventCamera();
        bool over = RectTransformUtility.RectangleContainsScreenPoint(viewport, screenPosition, eventCamera);

        HandleWheel(mouse, over);
        HandleDrag(mouse, screenPosition, eventCamera, over);
    }

    private void HandleWheel(Mouse mouse, bool over)
    {
        Vector2 scroll = mouse.scroll.ReadValue();

        if (!over || scroll == Vector2.zero)
            return;

        bool horizontal = Keyboard.current != null && Keyboard.current.shiftKey.isPressed;
        float amount = Mathf.Abs(scroll.y) > Mathf.Abs(scroll.x) ? scroll.y : scroll.x;

        // Molette vers le bas → on descend dans la grille (la grille remonte).
        Vector2 delta = horizontal
            ? new Vector2(Mathf.Sign(amount) * WheelStep, 0f)
            : new Vector2(0f, -Mathf.Sign(amount) * WheelStep);

        content.anchoredPosition += delta;
        Clamp();
    }

    private void HandleDrag(Mouse mouse, Vector2 screenPosition, Camera eventCamera, bool over)
    {
        if (mouse.leftButton.wasPressedThisFrame)
        {
            pressedInside = over;
            dragging = false;
            pressScreenPosition = screenPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(viewport, screenPosition, eventCamera, out lastLocalPoint);
        }

        if (!mouse.leftButton.isPressed)
        {
            pressedInside = false;
            dragging = false;
            return;
        }

        if (!pressedInside)
            return;

        // Un simple clic sur une case ne doit pas bouger la grille.
        if (!dragging && Vector2.Distance(screenPosition, pressScreenPosition) < DragThreshold)
            return;

        dragging = true;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(viewport, screenPosition, eventCamera, out Vector2 localPoint);
        content.anchoredPosition += localPoint - lastLocalPoint;
        lastLocalPoint = localPoint;
        Clamp();
    }

    private Camera EventCamera()
    {
        Canvas canvas = viewport.GetComponentInParent<Canvas>();

        if (canvas == null)
            return null;

        canvas = canvas.rootCanvas;
        return canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
    }

    public void Initialize(RectTransform gridContent)
    {
        if (initialized)
            return;

        content = gridContent;
        viewport = (RectTransform)transform;

        // La grille est étirée dans son cadre avec une marge (Size Delta négatif) : on la
        // retient pour savoir combien de place il y a, avant de redimensionner la grille.
        originalSizeDelta = content.sizeDelta;
        initialized = true;
    }

    /// <summary>Donne à la grille sa vraie taille, centrée dans le cadre.</summary>
    public void SetContentSize(Vector2 size)
    {
        content.anchorMin = content.anchorMax = content.pivot = new Vector2(0.5f, 0.5f);
        content.sizeDelta = size;
        content.anchoredPosition = Vector2.zero;
        Clamp();
    }

    /// <summary>Fait défiler juste ce qu'il faut pour que cette case soit entièrement visible.</summary>
    public void EnsureVisible(RectTransform tile)
    {
        if (!initialized || tile == null)
            return;

        // La grille place ses cases au moment du rendu : on la force à le faire maintenant.
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        Rect view = viewport.rect;
        Vector3[] corners = new Vector3[4];
        tile.GetWorldCorners(corners);

        Vector2 min = viewport.InverseTransformPoint(corners[0]);
        Vector2 max = viewport.InverseTransformPoint(corners[2]);
        Vector2 offset = Vector2.zero;

        if (min.x < view.xMin + VisibleMargin) offset.x = view.xMin + VisibleMargin - min.x;
        else if (max.x > view.xMax - VisibleMargin) offset.x = view.xMax - VisibleMargin - max.x;

        if (min.y < view.yMin + VisibleMargin) offset.y = view.yMin + VisibleMargin - min.y;
        else if (max.y > view.yMax - VisibleMargin) offset.y = view.yMax - VisibleMargin - max.y;

        if (offset == Vector2.zero)
            return;

        content.anchoredPosition += offset;
        Clamp();
    }

    /// <summary>Empêche de faire sortir la grille du cadre (et la centre si elle y tient).</summary>
    private void Clamp()
    {
        Vector2 limit = Vector2.Max(Vector2.zero, (content.sizeDelta - AvailableSize) / 2f);
        Vector2 position = content.anchoredPosition;

        position.x = Mathf.Clamp(position.x, -limit.x, limit.x);
        position.y = Mathf.Clamp(position.y, -limit.y, limit.y);

        content.anchoredPosition = position;
    }
}
