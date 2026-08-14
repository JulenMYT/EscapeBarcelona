using UnityEngine;

public class PuzzlePiece : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private BoxCollider2D boxCollider;

    public Vector2Int GridPosition { get; private set; }
    public Vector2 Size { get; private set; }
    private Bounds shuffleArea;

    private PuzzleConnectionManager connectionManager;

    public PuzzlePieceGroup Group { get; set; }

    private Vector3 dragOffset;

    public void Initialize(Sprite pieceSprite, Vector2Int gridPosition, PuzzleConnectionManager connectionManager, int sortingOrder, Bounds shuffleArea)
    {
        spriteRenderer.sprite = pieceSprite;
        spriteRenderer.sortingOrder = sortingOrder;
        GridPosition = gridPosition;
        this.connectionManager = connectionManager;
        this.shuffleArea = shuffleArea;

        SetupCollider();
    }

    private void SetupCollider()
    {
        Size = spriteRenderer.bounds.size;
        boxCollider.size = Size;
    }

    private void OnMouseDown()
    {
        dragOffset = Group.transform.position - GetMousePosition();
    }

    private void OnMouseDrag()
    {
        Vector3 position = GetMousePosition() + dragOffset;

        position.x = Mathf.Clamp(position.x, shuffleArea.min.x, shuffleArea.max.x);
        position.y = Mathf.Clamp(position.y, shuffleArea.min.y, shuffleArea.max.y);

        Group.Move(position);
    }

    private void OnMouseUp()
    {
        connectionManager.CheckConnection(Group);
    }

    private Vector3 GetMousePosition()
    {
        Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        position.z = 0;
        return position;
    }
}