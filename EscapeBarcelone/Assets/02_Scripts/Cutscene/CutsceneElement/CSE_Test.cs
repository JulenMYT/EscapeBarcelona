using UnityEngine;

public class CSE_Test : CutsceneElementBase
{
    public override void Execute()
    {
        StartCoroutine(WaitAndAdvance());
        Debug.Log("Executing " + name);
    }
}
