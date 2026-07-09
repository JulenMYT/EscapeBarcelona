using UnityEngine;

public class CutsceneHandler : MonoBehaviour
{
    public Camera cam { get; private set; }
    public DialogueHandler dialogueHandler { get; private set; }
    public TutorialHandler tutorialHandler { get; private set; }
    public FadePanel fadePanel { get; private set; }
    public FocusPanelTutorial focusPanel { get; private set; }

    private static DialogueHandler cachedDialogueHandler;
    private static TutorialHandler cachedTutorialHandler;
    private static FadePanel cachedFadePanel;
    private static FocusPanelTutorial cachedFocusPanel;

    private CutsceneElementBase[] cutsceneElements;
    private int index = -1;

    private void Start()
    {
        cam = Camera.main;
        cutsceneElements = GetComponentsInChildren<CutsceneElementBase>();

        if (cachedDialogueHandler == null)
            cachedDialogueHandler = FindAnyObjectByType<DialogueHandler>();

        if (cachedTutorialHandler == null)
            cachedTutorialHandler = FindAnyObjectByType<TutorialHandler>();

        if (cachedFadePanel == null)
            cachedFadePanel = FindAnyObjectByType<FadePanel>();

        if (cachedFocusPanel == null)
            cachedFocusPanel = FindAnyObjectByType<FocusPanelTutorial>();

        dialogueHandler = cachedDialogueHandler;
        tutorialHandler = cachedTutorialHandler;
        fadePanel = cachedFadePanel;
        focusPanel = cachedFocusPanel;
    }

    public void Restart()
    {
        index = -1;
    }

    private void ExecuteCurrentElement()
    {
        if (index >= 0 && index < cutsceneElements.Length)
        {
            cutsceneElements[index].Execute();
        }
        else if (index >= cutsceneElements.Length)
        {
            InteractionManager.SetBlocked(false);
        }
    }

    public void PlayNextElement()
    {
        index++;
        ExecuteCurrentElement();
    }
}