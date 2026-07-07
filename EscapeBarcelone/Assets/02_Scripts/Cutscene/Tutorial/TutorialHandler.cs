using System;
using UnityEngine;

public class TutorialHandler : MonoBehaviour
{
    private int index = -1;
    private Tutorial currentTutorial;
    private TutorialUI tutorialUI;

    public event Action OnTutorialComplete;

    private void Start()
    {
        tutorialUI = FindAnyObjectByType<TutorialUI>();

        if (!tutorialUI)
        {
            Debug.LogError("TutorialUI not found in the scene.");
        }
    }

    private void Update()
    {
        if (tutorialUI != null && Input.GetKeyDown(KeyCode.Mouse0))
        {
            PlayNextLine();
        }
    }

    public void StartTutorial(Tutorial tutorial)
    {
        currentTutorial = tutorial;
        index = -1;
        tutorialUI.Show();
        PlayNextLine();
    }

    private void PlayCurrentLine()
    {
        if (index >= 0 && index < currentTutorial.tutorialLines.Length)
        {
            tutorialUI.DisplayLine(currentTutorial.tutorialLines[index]);
        }

        if (index >= currentTutorial.tutorialLines.Length)
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        currentTutorial = null;
        tutorialUI.Hide();
        OnTutorialComplete?.Invoke();
    }

    public void PlayNextLine()
    {
        index++;
        PlayCurrentLine();
    }
}
