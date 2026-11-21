using UnityEngine;

public class MapLabel : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private PolygonCollider2D bounds;
    [SerializeField] private LayerMask anchorLayer;
    [SerializeField] private MapLabelGameManager gameManager;

    [SerializeField] private Color baseColor = Color.white;
    [SerializeField] private Color anchoredColor = Color.yellow;
    [SerializeField] private Color correctColor = Color.green;

    private Camera mainCamera;
    private Vector3 dragOffset;
    private MapLabelAnchor currentAnchor;

    private bool canDrag = true;

    // Les étiquettes et les ancres sont toutes à z = 0 dans la scène. OnMouseDown choisit
    // l'objet touché selon sa profondeur : à égalité, il peut toucher l'ancre au lieu de
    // l'étiquette posée dessus. On place donc l'étiquette un peu plus près de la caméra.
    private const float LabelDepthOffset = -0.1f;

    private void Awake()
    {
        Vector3 local = transform.localPosition;
        transform.localPosition = new Vector3(local.x, local.y, LabelDepthOffset);
    }

    public void SetCanDrag(bool canDrag)
    {
        this.canDrag = canDrag;
    }

    private void OnMouseDown()
    {
        if (CutsceneDirector.IsPlaying)
            return;

        if (!canDrag)
            return;

        if (currentAnchor != null)
        {
            currentAnchor.RemoveLabel();
            currentAnchor = null;
        }

        SetBaseColor();

        Vector3 mousePosition = GetMouseWorldPosition();
        dragOffset = transform.position - mousePosition;
    }

    private void OnMouseDrag()
    {
        if (CutsceneDirector.IsPlaying)
            return;

        if (!canDrag)
            return;

        Vector3 position = GetMouseWorldPosition() + dragOffset;

        if (bounds.OverlapPoint(position))
        {
            transform.position = position;
            return;
        }

        Vector2 closestPoint = bounds.ClosestPoint(position);

        transform.position = new Vector3(closestPoint.x, closestPoint.y, transform.position.z);
    }

    private void OnMouseUp()
    {
        MapLabelAnchor anchor = GetAnchorUnderMouse();

        if (anchor != null && anchor.TryPlace(this))
            currentAnchor = anchor;

        gameManager.CheckCompletion();
    }

    public void Anchor(MapLabelAnchor anchor)
    {
        currentAnchor = anchor;

        // On garde le z de l'étiquette : avec celui de l'ancre, les deux colliders sont à
        // la même profondeur, et OnMouseDown peut alors toucher l'ancre au lieu de
        // l'étiquette (impossible de la reprendre).
        Vector3 anchorPosition = anchor.transform.position;
        transform.position = new Vector3(anchorPosition.x, anchorPosition.y, transform.position.z);
        SetAnchoredColor();
    }

    public void SetCorrectColor()
    {
        spriteRenderer.color = correctColor;
    }

    private void SetBaseColor()
    {
        spriteRenderer.color = baseColor;
    }

    private void SetAnchoredColor()
    {
        spriteRenderer.color = anchoredColor;
    }

    private Vector3 GetMouseWorldPosition()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            Debug.Log("camera null");

        Vector3 position = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        position.z = transform.position.z;

        return position;
    }

    private MapLabelAnchor GetAnchorUnderMouse()
    {
        Vector3 mousePosition = GetMouseWorldPosition();

        Collider2D collider = Physics2D.OverlapPoint(mousePosition, anchorLayer);

        if (collider == null)
            return null;

        MapLabelAnchor anchor = collider.GetComponent<MapLabelAnchor>();

        return anchor;
    }
}