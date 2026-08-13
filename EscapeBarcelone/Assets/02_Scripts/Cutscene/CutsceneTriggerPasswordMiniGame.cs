using UnityEngine;

public class CutsceneTriggerPasswordMiniGame : CutsceneTriggerMiniGame
{
    [SerializeField] private Animator anim;

    protected override void OnGameCompleted()
    {
        ServiceLocator.Get<InspectionHandler>()
            .SetCanClose(false);

        anim.SetTrigger("open");
    }

    public void AnimationFinished()
    {
        cutscene.StartCutscene();
    }
}
