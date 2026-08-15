using System.Collections.Generic;
using UnityEngine;

public class CutsceneInteractionConditional : MonoBehaviour
{
    [SerializeField] private CutsceneInitiator cutscene;
    [SerializeField] private Interaction interaction;
    [SerializeField] private List<StoryFlag> requiredFlags;
    [SerializeField] private List<StoryFlag> forbiddenFlags;

    private void OnEnable()
    {
        interaction.OnClick += HandleInteraction;
    }

    private void OnDisable()
    {
        interaction.OnClick -= HandleInteraction;
    }

    private void HandleInteraction()
    {
        if (!CanPlay())
            return;

        cutscene.StartCutscene();
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
}