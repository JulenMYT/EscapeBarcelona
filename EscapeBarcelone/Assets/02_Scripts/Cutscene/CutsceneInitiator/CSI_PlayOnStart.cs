using UnityEngine;

public class CSI_PlayOnStart : CutsceneInitiator
{
    protected override void Initialize()
    {
        base.Initialize();

        StartCutscene();
    }
}
