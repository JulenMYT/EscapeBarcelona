using UnityEngine;

public class PuzzlePiecePickup : Inspectable
{
    [SerializeField] private PersistentGuid guid;

    private void Start()
    {
        if (ServiceLocator.Get<WorldState>().puzzlePiecesCollected.Contains(guid.Guid))
        {
            Destroy(gameObject);
        }
    }

    protected override void Interact()
    {
        base.Interact();

        ServiceLocator.Get<WorldState>().puzzlePiecesCollected.Add(guid.Guid);
        Destroy(gameObject);
    }
}
