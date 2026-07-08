using UnityEngine;

public class CSE_Wait : CutsceneElementBase
{
    public override void Execute()
    {
        StartCoroutine(WaitAndAdvance());
    }
}
