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

    private TextSequence sequence;
    private TextData line;
    private int lineIndex;
    private bool isPlaying;
    private bool isTransitioning;
    private Coroutine autoAdvanceRoutine;

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
        if (!CanReadManualAdvance())
        {
            return;
        }

        GoToNextLine();
    }

    public void Play(TextSequence newSequence)
    {
        StopAutoAdvance();

        sequence = newSequence;
        lineIndex = 0;
        isPlaying = true;

        ShowLineAtCurrentIndex();
    }

    public void Stop()
    {
        StopAutoAdvance();

        isPlaying = false;
        isTransitioning = false;

        HideView();
    }

    private void SubscribeToEvents()
    {
        if (inputField != null)
        {
            inputField.OnInputEntered += SubmitInput;
        }

        panel.OnFadeOutComplete += PrepareLineAfterFadeOut;
        panel.OnFadeInComplete += EnableLineInteractionAfterFadeIn;
    }

    private void UnsubscribeFromEvents()
    {
        if (inputField != null)
        {
            inputField.OnInputEntered -= SubmitInput;
        }

        panel.OnFadeOutComplete -= PrepareLineAfterFadeOut;
        panel.OnFadeInComplete -= EnableLineInteractionAfterFadeIn;
    }

    private void HideView()
    {
        panel.Hide();
        SetInputVisible(false);
        SetSpeakerVisible(false);
    }

    private bool CanReadManualAdvance()
    {
        if (!isPlaying || isTransitioning)
        {
            return false;
        }

        return Input.GetMouseButtonDown(0)
            || Input.GetKeyDown(KeyCode.Space)
            || Input.GetKeyDown(KeyCode.Return);
    }

    private void ShowLineAtCurrentIndex()
    {
        if (!HasLineAtCurrentIndex())
        {
            FinishSequence();
            return;
        }

        StopAutoAdvance();

        line = sequence.texts[lineIndex];
        isTransitioning = true;

        SetInputVisible(false);
        panel.FadeOut();
    }

    private bool HasLineAtCurrentIndex()
    {
        return sequence != null && lineIndex < sequence.texts.Length;
    }

    private void PrepareLineAfterFadeOut()
    {
        if (!isPlaying)
        {
            return;
        }

        StartCoroutine(ShowLineAfterDelay());
    }

    private IEnumerator ShowLineAfterDelay()
    {
        dialogueText.text = ResolveTextTags(line.text);
        ApplySpeaker(line.speakerName);
        SetInputVisible(line.inputField);

        yield return new WaitForSeconds(lineTransitionDelay);

        panel.FadeIn();
    }

    private void EnableLineInteractionAfterFadeIn()
    {
        if (!isPlaying)
        {
            return;
        }

        if (line.inputField && inputField != null)
        {
            return;
        }

        isTransitioning = line.autoAdvance;

        if (line.autoAdvance)
        {
            StartAutoAdvance(line.duration);
        }
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

    private string ResolveTextTags(string text)
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
        autoAdvanceRoutine = StartCoroutine(AutoAdvanceAfterDelay(duration));
    }

    private void StopAutoAdvance()
    {
        if (autoAdvanceRoutine == null)
        {
            return;
        }

        StopCoroutine(autoAdvanceRoutine);
        autoAdvanceRoutine = null;
    }

    private IEnumerator AutoAdvanceAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);

        autoAdvanceRoutine = null;
        GoToNextLine();
    }

    private void GoToNextLine()
    {
        lineIndex++;
        isTransitioning = false;

        ShowLineAtCurrentIndex();
    }

    private void SubmitInput(string value)
    {
        if (!isPlaying)
        {
            return;
        }

        OnInputEntered?.Invoke(value);

        SetInputVisible(false);
        GoToNextLine();
    }

    private void FinishSequence()
    {
        Stop();
        OnSequenceCompleted?.Invoke();
    }
}