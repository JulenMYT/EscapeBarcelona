using UnityEngine;

public class CSE_AddStoryFlag : CutsceneElementBase
{
    [SerializeField] private StoryFlag flag;

    public override void Execute()
    {
        ServiceLocator.Get<WorldState>()
            .storyFlags.Add(flag);

        cutsceneHandler.PlayNextElement();
    }
}