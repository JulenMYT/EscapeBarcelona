using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class TextSequenceHandler<T> : MonoBehaviour where T : ScriptableObject
{
    /// <summary>Déclenché à la fin de chaque séquence.</summary>
    public event Action OnSequenceComplete;

    [SerializeField] private InputActionReference inputAction;
    private bool inputBlocked;

    protected int index = -1;
    protected T currentSequence;

    // Séquences demandées pendant qu'une autre est en cours : jouées à la suite.
    private readonly Queue<(T sequence, Action onComplete)> pending = new();
    private Action currentOnComplete;

    public bool IsPlaying => currentSequence != null;

    protected virtual void OnEnable()
    {
        inputAction.action.performed += OnClick;

        // Activer est sans risque. En revanche on ne DÉSACTIVE jamais l'action dans
        // OnDisable : elle est partagée (dialogues, tutos, et peut-être le clic de l'UI),
        // la couper casserait les autres.
        inputAction.action.Enable();
    }

    protected virtual void OnDisable()
    {
        inputAction.action.performed -= OnClick;
    }

    /// <param name="onComplete">Appelé à la fin de CETTE séquence (même si elle a dû attendre son tour).</param>
    protected void StartSequence(T sequence, Action onComplete = null)
    {
        if (currentSequence != null)
        {
            Debug.LogWarning($"{GetType().Name} : « {sequence.name} » demandé pendant « {currentSequence.name} », il sera joué juste après.", this);
            pending.Enqueue((sequence, onComplete));
            return;
        }

        currentSequence = sequence;
        currentOnComplete = onComplete;
        index = -1;
        inputBlocked = true;

        OnBeginSequence(sequence);
        ShowUI();
        PlayNextLine();

        StartCoroutine(EnableInputNextFrame());
    }

    private IEnumerator EnableInputNextFrame()
    {
        yield return null;
        inputBlocked = false;
    }

    protected void PlayNextLine()
    {
        index++;

        if (index < GetLineCount())
        {
            DisplayCurrentLine();
        }
        else
        {
            EndSequence();
        }
    }

    private void EndSequence()
    {
        OnEndSequence();

        Action onComplete = currentOnComplete;
        currentSequence = null;
        currentOnComplete = null;

        onComplete?.Invoke();
        OnSequenceComplete?.Invoke();

        // Un callback a pu lancer une nouvelle séquence : on ne dépile que si on est libre.
        if (currentSequence == null && pending.Count > 0)
        {
            (T sequence, Action callback) = pending.Dequeue();
            StartSequence(sequence, callback);
        }
    }

    protected virtual bool CanAdvance()
    {
        return true;
    }

    /// <summary>Vrai tant que la réplique s'écrit encore (machine à écrire).</summary>
    protected virtual bool IsLineAnimating() => false;

    /// <summary>Affiche immédiatement toute la réplique en cours.</summary>
    protected virtual void CompleteLineAnimation() { }

    /// <summary>Appelé au début de chaque séquence, avant la première réplique.</summary>
    protected virtual void OnBeginSequence(T sequence) { }

    protected abstract int GetLineCount();
    protected abstract void DisplayCurrentLine();
    protected abstract void ShowUI();
    protected abstract void OnEndSequence();

    private void OnClick(InputAction.CallbackContext context)
    {
        if (inputBlocked || currentSequence == null)
            return;

        // Premier clic pendant l'écriture : affiche toute la réplique d'un coup.
        if (IsLineAnimating())
        {
            CompleteLineAnimation();
            return;
        }

        if (CanAdvance())
            PlayNextLine();
    }
}
