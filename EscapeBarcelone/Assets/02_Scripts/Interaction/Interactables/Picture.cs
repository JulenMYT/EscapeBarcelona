using TMPro;
using UnityEngine;

public class Picture : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private TMP_Text text;

    public void SetData(Sprite sprite, string text)
    {
        this.sprite.sprite = sprite;
        this.text.text = text;
    }
}