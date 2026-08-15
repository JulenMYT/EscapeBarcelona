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

    public void SetCanDrag(bool canDrag)
    {
        this.canDrag = canDrag;
    }

    private void OnMouseDown()
    {
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
        transform.position = anchor.transform.position;
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