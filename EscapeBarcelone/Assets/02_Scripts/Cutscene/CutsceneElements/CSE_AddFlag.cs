using UnityEngine;

public class CSE_AddFlag : CutsceneElementBase
{
    [SerializeField] private StoryFlag _storyFlag;

    public override void Execute()
    {
        base.Execute();

        StoryManager.SetFlag(_storyFlag);
        cutsceneHandler.PlayNextElement();
    }
}
