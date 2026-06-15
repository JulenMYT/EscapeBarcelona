using System;
using System.Collections;
using UnityEngine;

public class PanelUtils : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float fadeOutDuration = 1f;

    public event Action OnFadeInComplete;
    public event Action OnFadeOutComplete;

    public float FadeInDuration => fadeInDuration;
    public float FadeOutDuration => fadeOutDuration;

    private Coroutine currentFadeCoroutine;

    public void Display()
    {
        StopCurrentFade();

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        StopCurrentFade();

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void SetFadeInDuration(float duration)
    {
        fadeInDuration = duration;
    }

    public void SetFadeOutDuration(float duration)
    {
        fadeOutDuration = duration;
    }

    public void SetFadeDuration(float duration)
    {
        fadeInDuration = duration;
        fadeOutDuration = duration;
    }

    public void FadeIn()
    {
        StopCurrentFade();

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        currentFadeCoroutine = StartCoroutine(FadeCanvasGroup(
            canvasGroup,
            canvasGroup.alpha,
            1f,
            fadeInDuration,
            OnFadeInComplete));
    }

    public void FadeOut()
    {
        StopCurrentFade();

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        currentFadeCoroutine = StartCoroutine(FadeCanvasGroup(
            canvasGroup,
            canvasGroup.alpha,
            0f,
            fadeOutDuration,
            OnFadeOutComplete));
    }

    private void StopCurrentFade()
    {
        if (currentFadeCoroutine == null)
        {
            return;
        }

        StopCoroutine(currentFadeCoroutine);
        currentFadeCoroutine = null;
    }

    private IEnumerator FadeCanvasGroup(
        CanvasGroup cg,
        float start,
        float end,
        float duration,
        Action onComplete)
    {
        if (duration <= 0f)
        {
            cg.alpha = end;
            onComplete?.Invoke();
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }

        cg.alpha = end;

        currentFadeCoroutine = null;

        onComplete?.Invoke();
    }
}