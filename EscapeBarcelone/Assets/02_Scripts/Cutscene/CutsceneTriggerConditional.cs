using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneTriggerConditional : MonoBehaviour
{
    [SerializeField] private CutsceneInitiator cutscene;
    [SerializeField] private List<StoryFlag> requiredFlags;
    [SerializeField] private List<StoryFlag> forbiddenFlags;

    private void Start()
    {
        if (!CanPlay())
            return;

        StartCoroutine(WaitAndPlayCutscene());
    }

    private bool CanPlay()
    {
        WorldState worldState = ServiceLocator.Get<WorldState>();

        foreach (StoryFlag flag in requiredFlags)
        {
            if (!worldState.storyFlags.Contains(flag))
                return false;
        }

        foreach (StoryFlag flag in forbiddenFlags)
        {
            if (worldState.storyFlags.Contains(flag))
                return false;
        }

        return true;
    }

    private IEnumerator WaitAndPlayCutscene()
    {
        yield return null;

        cutscene.StartCutscene();
    }
}