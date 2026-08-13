using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class TextSequenceHandler<T> : MonoBehaviour where T : ScriptableObject
{
    protected int index = -1;
    protected T currentSequence;

    public event Action OnSequenceComplete;

    [SerializeField] private InputActionReference inputAction;
    private bool inputBlocked;

    protected virtual void OnEnable()
    {
        inputAction.action.performed += OnClick;
        inputAction.action.Enable();
    }

    protected virtual void OnDisable()
    {
        inputAction.action.performed -= OnClick;
        inputAction.action.Disable();
    }

    private void OnClick(InputAction.CallbackContext context)
    {
        if (inputBlocked)
            return;

        if (currentSequence != null && CanAdvance())
            PlayNextLine();
    }

    protected void StartSequence(T sequence)
    {
        currentSequence = sequence;
        index = -1;
        inputBlocked = true;

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

        currentSequence = null;
        OnSequenceComplete?.Invoke();
    }

    protected virtual bool CanAdvance()
    {
        return true;
    }

    protected abstract int GetLineCount();
    protected abstract void DisplayCurrentLine();
    protected abstract void ShowUI();
    protected abstract void OnEndSequence();
}