using TMPro;
using UnityEngine;

public class IE_DisplayPicture : InteractionElementBase
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private SpriteRenderer spriteRendererPicture;

    public void SetText(string newText)
    {
        text.text = newText;
    }

    public void SetSprite(Sprite newSprite)
    {
        spriteRendererPicture.sprite = newSprite;
    }
}
