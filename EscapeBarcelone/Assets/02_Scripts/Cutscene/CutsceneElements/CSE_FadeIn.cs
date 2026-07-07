using UnityEngine;

public class CSE_FadeIn : CutsceneElementBase
{
    public override void Execute()
    {
        cutsceneHandler.fadePanel.OnFadeInComplete += OnFadeComplete;
        cutsceneHandler.fadePanel.FadeIn(duration);
    }

    private void OnFadeComplete()
    {
        cutsceneHandler.fadePanel.OnFadeInComplete -= OnFadeComplete;
        cutsceneHandler.PlayNextElement();
    }
}
