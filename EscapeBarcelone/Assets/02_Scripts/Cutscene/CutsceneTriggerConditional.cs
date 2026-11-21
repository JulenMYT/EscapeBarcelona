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
        {
            return;
        }

        StartCoroutine(WaitAndPlayCutscene());
    }

    private bool CanPlay() => StoryCondition.Check(requiredFlags, forbiddenFlags);

    private IEnumerator WaitAndPlayCutscene()
    {
        yield return null;

        cutscene.StartCutscene();
    }
}