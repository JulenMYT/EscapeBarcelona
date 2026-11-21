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

    /// <param name="onComplete">Appelé à la fin de ce tuto précisément.</param>
    public void StartTutorial(Tutorial tutorial, Action onComplete = null)
    {
        StartSequence(tutorial, onComplete);
    }

    // Ici (et non dans StartTutorial) : si le tuto a dû attendre son tour,
    // la position doit être appliquée au moment où il démarre vraiment.
    protected override void OnBeginSequence(Tutorial tutorial)
    {
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

    protected override bool IsLineAnimating() => tutorialUI.IsTyping;

    protected override void CompleteLineAnimation() => tutorialUI.CompleteLine();

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
        waitingForInteraction = false;
        tutorialUI.Hide();
        tutorialOverlay.SetActive(false);
    }

    protected override void ShowUI()
    {
        tutorialUI.Show();
    }
}
