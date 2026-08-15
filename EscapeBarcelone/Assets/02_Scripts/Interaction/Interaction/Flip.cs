using UnityEngine;

public class Flip : Interaction
{
    [SerializeField] private GameObject flipOtherSide;

    protected override void Interact()
    {
        flipOtherSide.SetActive(true);
        gameObject.SetActive(false);
    }
}
