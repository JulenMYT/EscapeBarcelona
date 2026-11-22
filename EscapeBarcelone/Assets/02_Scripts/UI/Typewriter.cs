using System;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Effet machine à écrire sur un TMP_Text : les lettres apparaissent une à une.
/// Utilise maxVisibleCharacters, donc les balises de mise en forme (&lt;b&gt;, &lt;color&gt;…)
/// fonctionnent et ne sont jamais affichées à moitié.
/// </summary>
[Serializable]
public class Typewriter
{
    /// <summary>Déclenché quand la réplique est entièrement affichée.</summary>
    public event Action Completed;

    [Tooltip("Lettres par seconde. 0 = texte affiché d'un coup.")]
    [SerializeField] private float charactersPerSecond = 40f;

    [Tooltip("Petite pause après . ! ? … (en secondes).")]
    [SerializeField] private float sentencePause = 0.2f;

    [Tooltip("Petite pause après , ; : (en secondes).")]
    [SerializeField] private float commaPause = 0.08f;

    private TMP_Text text;
    private MonoBehaviour host;
    private Coroutine routine;

    public bool IsTyping => routine != null;

    public void Play(MonoBehaviour host, TMP_Text text, string content)
    {
        Stop();

        this.host = host;
        this.text = text;

        text.text = content;

        if (charactersPerSecond <= 0f || !host.isActiveAndEnabled)
        {
            ShowAll();
            return;
        }

        text.maxVisibleCharacters = 0;
        routine = host.StartCoroutine(TypeRoutine());
    }

    private IEnumerator TypeRoutine()
    {
        // Calcule les caractères réellement affichés (sans les balises).
        text.ForceMeshUpdate();

        TMP_TextInfo info = text.textInfo;
        int total = info.characterCount;
        float delay = 1f / charactersPerSecond;

        for (int visible = 1; visible <= total; visible++)
        {
            text.maxVisibleCharacters = visible;

            char c = info.characterInfo[visible - 1].character;
            float wait = delay;

            // Pas de pause sur la toute dernière lettre.
            if (visible < total)
            {
                if (c == '.' || c == '!' || c == '?' || c == '…')
                    wait += sentencePause;
                else if (c == ',' || c == ';' || c == ':')
                    wait += commaPause;
            }

            float elapsed = 0f;

            while (elapsed < wait)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        routine = null;
        ShowAll();
    }

    /// <summary>Affiche immédiatement toute la réplique.</summary>
    public void Complete()
    {
        if (!IsTyping)
            return;

        Stop();
        ShowAll();
    }

    public void Stop()
    {
        if (routine != null && host != null)
            host.StopCoroutine(routine);

        routine = null;
    }

    private void ShowAll()
    {
        if (text != null)
            text.maxVisibleCharacters = int.MaxValue;

        Completed?.Invoke();
    }
}
