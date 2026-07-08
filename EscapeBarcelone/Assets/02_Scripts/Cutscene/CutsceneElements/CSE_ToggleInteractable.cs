using UnityEngine;

public class CSE_ToggleInteractable : CutsceneElementBase
{
    [SerializeField] private InteractionElementBase targetObject;

    [SerializeField] private bool setInteractable = true;

    public override void Execute()
    {
        base.Execute();
        targetObject.SetInteractable(setInteractable);
    }
}
