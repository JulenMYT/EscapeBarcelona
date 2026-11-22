using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Étiquette de la carte qu'on glisse sur une ancre.
/// Le glisser-déposer passe par l'EventSystem (comme le reste du jeu) : l'UI posée
/// par-dessus bloque les clics, et l'objet attrapé est celui qui est affiché au-dessus
/// (ordre d'affichage), sans astuce de profondeur.
/// </summary>
public class MapLabel : MonoBehaviour, IPointerDownHandler, IInitializePotentialDragHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private PolygonCollider2D bounds;
    [SerializeField] private LayerMask anchorLayer;
    [SerializeField] private MapLabelGameManager gameManager;

    [SerializeField] private Color baseColor = Color.white;
    [SerializeField] private Color anchoredColor = Color.yellow;
    [SerializeField] private Color correctColor = Color.green;

    private Vector3 dragOffset;
    private MapLabelAnchor currentAnchor;

    private bool canDrag = true;
    private bool isDragging;

    public void SetCanDrag(bool canDrag)
    {
        this.canDrag = canDrag;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || !canDrag || CutsceneDirector.IsPlaying)
            return;

        isDragging = true;

        if (currentAnchor != null)
        {
            currentAnchor.RemoveLabel();
            currentAnchor = null;
        }

        SetBaseColor();

        dragOffset = transform.position - ToWorld(eventData);
    }

    private void SetBaseColor()
    {
        spriteRenderer.color = baseColor;
    }

    // Le glissement commence tout de suite, sans le petit seuil de l'EventSystem.
    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        eventData.useDragThreshold = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || !canDrag || CutsceneDirector.IsPlaying)
            return;

        Vector3 position = ToWorld(eventData) + dragOffset;

        if (bounds.OverlapPoint(position))
        {
            transform.position = position;
            return;
        }

        Vector2 closestPoint = bounds.ClosestPoint(position);

        transform.position = new Vector3(closestPoint.x, closestPoint.y, transform.position.z);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        isDragging = false;

        MapLabelAnchor anchor = GetAnchorUnder(ToWorld(eventData));

        if (anchor != null && anchor.TryPlace(this))
            currentAnchor = anchor;

        gameManager.CheckCompletion();
    }

    private MapLabelAnchor GetAnchorUnder(Vector3 worldPosition)
    {
        Collider2D collider = Physics2D.OverlapPoint(worldPosition, anchorLayer);

        return collider != null ? collider.GetComponent<MapLabelAnchor>() : null;
    }

    public void Anchor(MapLabelAnchor anchor)
    {
        currentAnchor = anchor;

        Vector3 anchorPosition = anchor.transform.position;
        transform.position = new Vector3(anchorPosition.x, anchorPosition.y, transform.position.z);
        SetAnchoredColor();
    }

    private void SetAnchoredColor()
    {
        spriteRenderer.color = anchoredColor;
    }

    public void SetCorrectColor()
    {
        spriteRenderer.color = correctColor;
    }

    private Vector3 ToWorld(PointerEventData eventData)
    {
        Camera cam = eventData.pressEventCamera != null ? eventData.pressEventCamera : Camera.main;

        Vector3 position = cam.ScreenToWorldPoint(eventData.position);
        position.z = transform.position.z;

        return position;
    }
}
