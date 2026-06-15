using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TextSequenceViewer : MonoBehaviour
{
    private const string PlayerTag = "{player}";
    private const string PlayerSpeakerName = "Player";

    [SerializeField] private EnterTextInputField inputField;
    [SerializeField] private PanelUtils panel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private float lineTransitionDelay = 0.1f;

    public event Action OnSequenceCompleted;
    public event Action<string> OnInputEntered;
    public event Action<string> OnTextEvent;

    private TextSequence currentSequence;
    private TextData currentLine;
    private int currentLineIndex;
    private bool isPlaying;
    private bool isTransitioning;
    private Coroutine autoAdvanceCoroutine;
    private Coroutine lineDelayCoroutine;

    private void Awake()
    {
        HideView();
    }

    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    private void Update()
    {
        if (ShouldAdvanceManually())
        {
            AdvanceToNextLine();
        }
    }

    public void Play(TextSequence sequence)
    {
        StopRunningCoroutines();

        currentSequence = sequence;
        currentLineIndex = 0;
        isPlaying = true;
        isTransitioning = false;

        BeginCurrentLine();
    }

    public void Stop()
    {
        StopRunningCoroutines();

        isPlaying = false;
        isTransitioning = false;
        currentSequence = null;

        HideView();
    }

    public void ContinueAfterExternalAction()
    {
        if (!CanContinueAfterExternalAction())
        {
            return;
        }

        AdvanceToNextLine();
    }

    private void SubscribeToEvents()
    {
        if (inputField != null)
        {
            inputField.OnInputEntered += HandleInputSubmitted;
        }

        panel.OnFadeOutComplete += HandlePanelFadeOutComplete;
        panel.OnFadeInComplete += HandlePanelFadeInComplete;
    }

    private void UnsubscribeFromEvents()
    {
        if (inputField != null)
        {
            inputField.OnInputEntered -= HandleInputSubmitted;
        }

        panel.OnFadeOutComplete -= HandlePanelFadeOutComplete;
        panel.OnFadeInComplete -= HandlePanelFadeInComplete;
    }

    private void HideView()
    {
        panel.Hide();
        SetInputVisible(false);
        SetSpeakerVisible(false);
    }

    private bool ShouldAdvanceManually()
    {
        if (!isPlaying || isTransitioning)
        {
            return false;
        }

        return Input.GetMouseButtonDown(0)
            || Input.GetKeyDown(KeyCode.Space)
            || Input.GetKeyDown(KeyCode.Return);
    }

    private void BeginCurrentLine()
    {
        if (!TryLoadCurrentLine())
        {
            CompleteSequence();
            return;
        }

        StopRunningCoroutines();

        isTransitioning = true;
        SetInputVisible(false);

        panel.FadeOut();
    }

    private bool TryLoadCurrentLine()
    {
        if (currentSequence == null || currentLineIndex >= currentSequence.texts.Length)
        {
            return false;
        }

        currentLine = currentSequence.texts[currentLineIndex];
        return true;
    }

    private void HandlePanelFadeOutComplete()
    {
        if (!isPlaying)
        {
            return;
        }

        StartLineRevealDelay();
    }

    private void StartLineRevealDelay()
    {
        StopLineDelay();

        lineDelayCoroutine = StartCoroutine(RevealLineAfterDelay());
    }

    private IEnumerator RevealLineAfterDelay()
    {
        yield return new WaitForSeconds(lineTransitionDelay);

        lineDelayCoroutine = null;

        ApplyCurrentLineToView();
        panel.FadeIn();
    }

    private void ApplyCurrentLineToView()
    {
        dialogueText.text = ResolveText(currentLine.text);
        ApplySpeaker(currentLine.speakerName);
        SetInputVisible(ShouldShowInput());
    }

    private void HandlePanelFadeInComplete()
    {
        if (!isPlaying)
        {
            return;
        }

        TriggerCurrentLineEvent();
        ConfigureCurrentLineInteraction();
    }

    private void ConfigureCurrentLineInteraction()
    {
        if (ShouldWaitForInput())
        {
            return;
        }

        if (ShouldWaitForExternalAction())
        {
            isTransitioning = true;
            return;
        }

        if (ShouldAutoAdvance())
        {
            isTransitioning = true;
            StartAutoAdvance(currentLine.duration);
            return;
        }

        isTransitioning = false;
    }

    private bool ShouldShowInput()
    {
        return currentLine.inputField && inputField != null;
    }

    private bool ShouldWaitForInput()
    {
        return currentLine.inputField && inputField != null;
    }

    private bool ShouldWaitForExternalAction()
    {
        return currentLine.waitForExternalAction;
    }

    private bool ShouldAutoAdvance()
    {
        return currentLine.autoAdvance;
    }

    private bool CanContinueAfterExternalAction()
    {
        return isPlaying && currentLine.waitForExternalAction;
    }

    private void TriggerCurrentLineEvent()
    {
        if (string.IsNullOrWhiteSpace(currentLine.eventKey))
        {
            return;
        }

        OnTextEvent?.Invoke(currentLine.eventKey);
    }

    private void ApplySpeaker(string speakerName)
    {
        if (speakerText == null)
        {
            return;
        }

        bool hasSpeaker = !string.IsNullOrWhiteSpace(speakerName);
        SetSpeakerVisible(hasSpeaker);

        if (!hasSpeaker)
        {
            return;
        }

        speakerText.text = ResolveSpeakerName(speakerName);
    }

    private string ResolveSpeakerName(string speakerName)
    {
        return speakerName == PlayerSpeakerName
            ? GameManager.Instance.playerName
            : speakerName;
    }

    private string ResolveText(string text)
    {
        return text.Replace(PlayerTag, GameManager.Instance.playerName);
    }

    private void SetInputVisible(bool visible)
    {
        if (inputField == null)
        {
            return;
        }

        inputField.gameObject.SetActive(visible);
    }

    private void SetSpeakerVisible(bool visible)
    {
        if (speakerText == null)
        {
            return;
        }

        speakerText.gameObject.SetActive(visible);
    }

    private void StartAutoAdvance(float duration)
    {
        StopAutoAdvance();

        autoAdvanceCoroutine = StartCoroutine(AutoAdvanceAfterDelay(duration));
    }

    private IEnumerator AutoAdvanceAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);

        autoAdvanceCoroutine = null;
        AdvanceToNextLine();
    }

    private void StopRunningCoroutines()
    {
        StopAutoAdvance();
        StopLineDelay();
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

    private void StopLineDelay()
    {
        if (lineDelayCoroutine == null)
        {
            return;
        }

        StopCoroutine(lineDelayCoroutine);
        lineDelayCoroutine = null;
    }

    private void AdvanceToNextLine()
    {
        currentLineIndex++;
        isTransitioning = false;

        BeginCurrentLine();
    }

    private void HandleInputSubmitted(string value)
    {
        if (!isPlaying)
        {
            return;
        }

        OnInputEntered?.Invoke(value);

        SetInputVisible(false);
        AdvanceToNextLine();
    }

    private void CompleteSequence()
    {
        Stop();
        OnSequenceCompleted?.Invoke();
    }
}