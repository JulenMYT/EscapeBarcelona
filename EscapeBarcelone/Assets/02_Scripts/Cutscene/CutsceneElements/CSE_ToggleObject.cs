using UnityEngine;

public class CSE_ToggleObject : CutsceneElementBase
{
    [SerializeField] private GameObject target;
    [SerializeField] private bool enable;
    public override void Execute()
    {
        if (target)
        {
            target.SetActive(enable);
        }
        cutsceneHandler.PlayNextElement();
    }
}
