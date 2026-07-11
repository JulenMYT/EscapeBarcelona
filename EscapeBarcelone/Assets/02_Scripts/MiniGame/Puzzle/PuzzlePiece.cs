using UnityEngine;

public class PuzzlePiece : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private BoxCollider2D boxCollider;

    public Vector2Int GridPosition { get; private set; }

    public PuzzlePieceGroup Group { get; set; }

    public Vector2 Size { get; private set; }


    private PuzzleConnectionManager connectionManager;

    private Vector3 dragOffset;

    public void Initialize(
        Sprite pieceSprite,
        Vector2Int gridPosition,
        PuzzleConnectionManager connectionManager)
    {
        spriteRenderer.sprite = pieceSprite;

        GridPosition = gridPosition;

        this.connectionManager = connectionManager;

        SetupCollider();
    }

    private void SetupCollider()
    {
        Size = spriteRenderer.bounds.size;

        boxCollider.size = Size;
    }

    private void OnMouseDown()
    {
        dragOffset =
            Group.transform.position - GetMousePosition();
    }

    private void OnMouseDrag()
    {
        Group.Move(
            GetMousePosition() + dragOffset
        );
    }

    private void OnMouseUp()
    {
        connectionManager.CheckConnection(Group);
    }

    private Vector3 GetMousePosition()
    {
        Vector3 position =
            Camera.main.ScreenToWorldPoint(
                Input.mousePosition
            );

        position.z = 0;

        return position;
    }
}