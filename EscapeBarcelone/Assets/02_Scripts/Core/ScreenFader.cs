using System.Collections;
using UnityEngine;

public class ScreenFader : MonoBehaviour
{
    // Doit passer au-dessus de tous les autres canvas (Book/Compass = 90, Dialogue/Tutorial = 100).
    private const int FadeSortingOrder = 1000;

    public CanvasGroup canvasGroup;

    private void Awake()
    {
        Canvas canvas = canvasGroup.GetComponentInParent<Canvas>();

        if (canvas != null)
            canvas.sortingOrder = FadeSortingOrder;
    }

    public IEnumerator Fade(float start, float end, float duration)
    {
        // Pendant le fondu, l'écran noir bloque aussi les clics sur l'UI (livre, boussole…).
        canvasGroup.blocksRaycasts = true;

        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, end, t / duration);
            yield return null;
        }

        SetAlpha(end);
    }

    public void SetAlpha(float alpha)
    {
        canvasGroup.alpha = alpha;
        canvasGroup.blocksRaycasts = alpha > 0f;
    }
}
