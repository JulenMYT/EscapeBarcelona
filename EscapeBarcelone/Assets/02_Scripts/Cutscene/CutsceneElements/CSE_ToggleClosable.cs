using System.Collections.Generic;
using UnityEngine;

public class CSE_ToggleClosable : CutsceneElementBase
{
    [SerializeField] private List<InteractionElementBase> targetObjects;

    [SerializeField] private bool setClosable = true;

    public override void Execute()
    {
        base.Execute();

        foreach (InteractionElementBase targetObject in targetObjects)
        {
            targetObject.SetClosable(setClosable);
        }
        cutsceneHandler.PlayNextElement();
    }
}