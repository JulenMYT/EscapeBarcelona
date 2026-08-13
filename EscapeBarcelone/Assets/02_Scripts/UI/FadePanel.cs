using System;
using System.Collections;
using UnityEngine;

public class FadePanel : Panel
{
    public event Action OnFadeInComplete;
    public event Action OnFadeOutComplete;

    private Coroutine fadeCoroutine;

    public void FadeIn(float duration)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(Fade(0f, 1f, duration, OnFadeInComplete));
    }

    public void FadeOut(float duration)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(Fade(1f, 0f, duration, OnFadeOutComplete));
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, float duration, Action onComplete)
    {
        if (duration <= 0f)
        {
            SetAlpha(endAlpha);
            fadeCoroutine = null;
            onComplete?.Invoke();
            yield break;
        }

        float startTime = Time.time;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            float alpha = Mathf.Lerp(startAlpha, endAlpha, t);

            SetAlpha(alpha);

            elapsedTime = Time.time - startTime;

            yield return null;
        }

        SetAlpha(endAlpha);
        fadeCoroutine = null;
        onComplete?.Invoke();
    }

    private void SetAlpha(float alpha)
    {
        canvasGroup.alpha = alpha;
    }
}
