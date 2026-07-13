using UnityEngine;

public class IE_Flip : InteractionElementBase
{
    [SerializeField] GameObject frontObject;
    [SerializeField] GameObject backObject;

    private bool frontSide = true;

    public override void Open()
    {
        base.Open();

        Flip(true);
    }

    public override void Interact()
    {
        base.Interact();

        Flip(!frontSide);
    }

    private void Flip(bool isFirstSide)
    {
        frontSide = isFirstSide;
        frontObject.SetActive(frontSide);
        backObject.SetActive(!frontSide);
    }
}
