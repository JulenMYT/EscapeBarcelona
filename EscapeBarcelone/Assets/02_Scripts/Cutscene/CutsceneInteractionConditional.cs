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

    private bool CanPlay() => StoryCondition.Check(requiredFlags, forbiddenFlags);
}