using UnityEngine;

public class CSE_FadeOut : CutsceneElementBase
{
    public override void Execute()
    {
        cutsceneHandler.fadePanel.OnFadeOutComplete += OnFadeComplete;
        cutsceneHandler.fadePanel.FadeOut(duration);
    }

    private void OnFadeComplete()
    {
        cutsceneHandler.fadePanel.OnFadeOutComplete -= OnFadeComplete;
        cutsceneHandler.PlayNextElement();
    }
}
