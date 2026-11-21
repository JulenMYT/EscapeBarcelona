using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Pièce de puzzle. Le glisser-déposer passe par l'EventSystem : l'UI bloque les clics, et
/// la pièce attrapée est celle affichée devant (même ordre d'affichage : la plus proche
/// de la caméra, réglée par PuzzleConnectionManager.BringToFront).
/// </summary>
public class PuzzlePiece : MonoBehaviour, IPointerDownHandler, IInitializePotentialDragHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private BoxCollider2D boxCollider;

    public Vector2Int GridPosition { get; private set; }
    public Vector2 Size { get; private set; }
    private Bounds shuffleArea;

    private PuzzleConnectionManager connectionManager;

    public PuzzlePieceGroup Group { get; set; }

    private Vector3 dragOffset;
    private bool isDragging;

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

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || CutsceneDirector.IsPlaying)
            return;

        isDragging = true;

        // Le groupe attrapé passe devant tous les autres (affichage ET clic) et y reste.
        connectionManager.BringToFront(Group);

        dragOffset = Group.transform.position - ToWorld(eventData);
    }

    // Le glissement commence tout de suite, sans le petit seuil de l'EventSystem.
    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        eventData.useDragThreshold = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || CutsceneDirector.IsPlaying)
            return;

        Vector3 position = ToWorld(eventData) + dragOffset;

        Group.Move(Group.ClampToArea(position, shuffleArea));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        isDragging = false;
        connectionManager.CheckConnection(Group);
    }

    private static Vector3 ToWorld(PointerEventData eventData)
    {
        Camera cam = eventData.pressEventCamera != null ? eventData.pressEventCamera : Camera.main;
        Vector3 position = cam.ScreenToWorldPoint(eventData.position);

        position.z = 0;
        return position;
    }
}
