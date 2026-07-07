using System;
using UnityEngine;

public class CutsceneHandler : MonoBehaviour
{
    public Camera cam;
    public DialogueHandler dialogueHandler { get; private set; }
    private CutsceneElementBase[] cutsceneElements;
    private int index = -1;

    public void Start()
    {
        cutsceneElements = GetComponentsInChildren<CutsceneElementBase>();
        dialogueHandler = FindAnyObjectByType<DialogueHandler>();
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
