using UnityEngine;

public class Inspectable : Interaction
{
    [SerializeField] protected GameObject detailedView;

    protected override void Interact()
    {
        ServiceLocator.Get<InspectionHandler>().Inspect(detailedView);
    }
}