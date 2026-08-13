using Unity.VisualScripting;
using UnityEngine;

public class CutsceneHandler : MonoBehaviour
{
    public Camera cam { get; private set; }
    public DialogueHandler dialogueHandler { get; private set; }
    public TutorialHandler tutorialHandler { get; private set; }

    private CutsceneElementBase[] cutsceneElements;
    private int index = -1;

    private void Awake()
    {
        cam = Camera.main;
        cutsceneElements = GetComponentsInChildren<CutsceneElementBase>();
    }

    private void Start()
    { 
        dialogueHandler = ServiceLocator.Get<DialogueHandler>();
        tutorialHandler = ServiceLocator.Get<TutorialHandler>();
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
            ServiceLocator.Get<InteractionManager>().Unlock();
        }
    }

    public void PlayNextElement()
    {
        index++;
        ExecuteCurrentElement();
    }
}