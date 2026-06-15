using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TextSequenceViewer : MonoBehaviour
{
    [SerializeField] private EnterTextInputField inputField;
    [SerializeField] private PanelUtils panel;
    [SerializeField] private TMP_Text textLabel;

    public event Action OnSequenceCompleted;
    public event Action<string> OnInputEntered;

    private TextSequence currentSequence;
    private TextData currentEntry;
    private int currentIndex;
    private bool isPlaying;
    private bool isBlocked;
    private Coroutine autoAdvanceCoroutine;

    private void Awake()
    {
        panel.Hide();
        inputField.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        inputField.OnInputEntered += HandleInputEntered;
        panel.OnFadeOutComplete += HandleFadeOutComplete;
        panel.OnFadeInComplete += HandleFadeInComplete;
    }

    private void OnDisable()
    {
        inputField.OnInputEntered -= HandleInputEntered;
        panel.OnFadeOutComplete -= HandleFadeOutComplete;
        panel.OnFadeInComplete -= HandleFadeInComplete;
    }

    private void Update()
    {
        if (!isPlaying || isBlocked)
        {
            return;
        }

        if (CanAdvanceManually())
        {
            Advance();
        }
    }

    public void Play(TextSequence sequence)
    {
        StopAutoAdvance();

        currentSequence = sequence;
        currentIndex = 0;
        isPlaying = true;

        ShowCurrentEntry();
    }

    public void Stop()
    {
        StopAutoAdvance();

        isPlaying = false;
        isBlocked = false;

        inputField.gameObject.SetActive(false);
        panel.Hide();
    }

    private bool CanAdvanceManually()
    {
        return Input.GetMouseButtonDown(0)
            || Input.GetKeyDown(KeyCode.Space)
            || Input.GetKeyDown(KeyCode.Return);
    }

    private void ShowCurrentEntry()
    {
        if (currentSequence == null || currentIndex >= currentSequence.texts.Length)
        {
            CompleteSequence();
            return;
        }

        StopAutoAdvance();

        currentEntry = currentSequence.texts[currentIndex];
        isBlocked = true;

        inputField.gameObject.SetActive(false);
        panel.FadeOut();
    }

    private void HandleFadeOutComplete()
    {
        if (!isPlaying)
        {
            return;
        }

        textLabel.text = currentEntry.text;
        inputField.gameObject.SetActive(currentEntry.inputField);

        panel.FadeIn();
    }

    private void HandleFadeInComplete()
    {
        if (!isPlaying)
        {
            return;
        }

        if (currentEntry.inputField)
        {
            return;
        }

        isBlocked = currentEntry.autoAdvance;

        if (currentEntry.autoAdvance)
        {
            StartAutoAdvance(currentEntry.duration);
        }
    }

    private void StartAutoAdvance(float duration)
    {
        StopAutoAdvance();
        autoAdvanceCoroutine = StartCoroutine(AutoAdvance(duration));
    }

    private void StopAutoAdvance()
    {
        if (autoAdvanceCoroutine == null)
        {
            return;
        }

        StopCoroutine(autoAdvanceCoroutine);
        autoAdvanceCoroutine = null;
    }

    private IEnumerator AutoAdvance(float duration)
    {
        yield return new WaitForSeconds(duration);

        autoAdvanceCoroutine = null;
        Advance();
    }

    private void Advance()
    {
        currentIndex++;
        isBlocked = false;

        ShowCurrentEntry();
    }

    private void HandleInputEntered(string value)
    {
        if (!isPlaying)
        {
            return;
        }

        OnInputEntered?.Invoke(value);

        inputField.gameObject.SetActive(false);
        Advance();
    }

    private void CompleteSequence()
    {
        Stop();

        OnSequenceCompleted?.Invoke();
    }
}