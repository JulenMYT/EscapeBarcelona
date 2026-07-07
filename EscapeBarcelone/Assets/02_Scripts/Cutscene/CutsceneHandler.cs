using System;
using UnityEngine;

public class CutsceneHandler : MonoBehaviour
{
    public Camera cam;
    private CutsceneElementBase[] cutsceneElements;
    private int index = -1;

    public void Start()
    {
        cutsceneElements = GetComponentsInChildren<CutsceneElementBase>();
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
