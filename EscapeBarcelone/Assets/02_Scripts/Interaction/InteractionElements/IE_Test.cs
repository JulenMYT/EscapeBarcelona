using UnityEngine;

public class IE_Test : InteractionElementBase
{
    public override void Interact()
    {
        base.Interact();
        Debug.Log("IE_Test " + name);
    }
}
