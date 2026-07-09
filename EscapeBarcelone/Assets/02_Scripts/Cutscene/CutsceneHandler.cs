using System;
using UnityEngine;

public class CutsceneHandler : MonoBehaviour
{
    public Camera cam { get; private set; }
    public DialogueHandler dialogueHandler { get; private set; }
    public TutorialHandler tutorialHandler { get; private set; }
    public FadePanel fadePanel { get; private set; }
    public FocusPanelTutorial focusPanel { get; private set; }
    private CutsceneElementBase[] cutsceneElements;
    private int index = -1;

    public void Start()
    {
        cam = Camera.main;
        cutsceneElements = GetComponentsInChildren<CutsceneElementBase>();
        dialogueHandler = FindAnyObjectByType<DialogueHandler>();
        tutorialHandler = FindAnyObjectByType<TutorialHandler>();
        fadePanel = FindAnyObjectByType<FadePanel>();
        focusPanel = FindAnyObjectByType<FocusPanelTutorial>();
    }

    private void ExecuteCurrentElement()
    {
        if (index >= 0 && index < cutsceneElements.Length)
        {
            cutsceneElements[index].Execute();
        }
    }

    public void PlayNextElement()
    {
        index++;
        ExecuteCurrentElement();
    }
}
