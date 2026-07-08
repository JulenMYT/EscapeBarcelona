using UnityEngine;

public class IE_Pictures : IE_ZoomIn
{
    [SerializeField] private Sprite sprite;
    [TextArea(3, 10)]
    [SerializeField] private string text;

    protected override void BeforeOpenTarget()
    {
        IE_DisplayPicture displayPicture = targetObject as IE_DisplayPicture;

        if (displayPicture)
        {
            displayPicture.SetSprite(sprite);
            displayPicture.SetText(text);
        }
    }
}