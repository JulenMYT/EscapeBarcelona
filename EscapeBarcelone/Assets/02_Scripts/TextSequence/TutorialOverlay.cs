using UnityEngine;

public class TutorialOverlay : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    public void SetActive(bool active)
    {
        spriteRenderer.enabled = active;
    }
}