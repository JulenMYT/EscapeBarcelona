using UnityEngine;

public class IE_PuzzlePiece : IE_ZoomIn
{
    [SerializeField] private PuzzleMiniGame puzzleMiniGame;

    public override void Interact()
    {
        base.Interact();

        //puzzleMiniGame.RevealPiece();
    }

    protected override void HandleClose()
    {
        base.HandleClose();

        Destroy(gameObject);
    }
}
