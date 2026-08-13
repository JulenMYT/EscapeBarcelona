using System;
using UnityEngine;

public class TutorialHandler : TextSequenceHandler<Tutorial>
{
    [SerializeField] private TutorialUI tutorialUI;
    [SerializeField] private GameObject tutorialOverlay;

    private bool waitingForInteraction;

    public event Action OnInteractionRequired;

    private void Awake()
    {
        ServiceLocator.Register<TutorialHandler>(this);
    }

    public void StartTutorial(Tutorial tutorial)
    {
        StartSequence(tutorial);
        tutorialUI.SetTextPosition(tutorial.textPos);
    }

    protected override int GetLineCount()
    {
        return currentSequence.tutorialLines.Length;
    }

    protected override void DisplayCurrentLine()
    {
        TutorialLine line = currentSequence.tutorialLines[index];

        tutorialUI.DisplayLine(line.text);

        tutorialOverlay.SetActive(line.waitForInteraction);

        waitingForInteraction = line.waitForInteraction;

        if (waitingForInteraction)
            OnInteractionRequired?.Invoke();
    }

    protected override bool CanAdvance()
    {
        return !waitingForInteraction;
    }

    public void CompleteInteraction()
    {
        if (!waitingForInteraction)
            return;

        waitingForInteraction = false;
        PlayNextLine();
    }

    protected override void OnEndSequence()
    {
        tutorialUI.Hide();
        tutorialOverlay.SetActive(false);
    }

    protected override void ShowUI()
    {
        tutorialUI.Show();
    }
}