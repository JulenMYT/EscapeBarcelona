using UnityEngine;

public class PictureInspectable : Inspectable
{
    [SerializeField] private Sprite picture;
    [SerializeField] private string description;

    protected override void Interact()
    {
        Picture pictures = detailedView.GetComponentInChildren<Picture>();

        pictures.SetData(picture, description);

        base.Interact();
    }
}