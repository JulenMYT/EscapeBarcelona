using System.Collections.Generic;
using UnityEngine;

public class CSI_PlayOnFlag : CSI_SceneLoadedBase
{
    [SerializeField] private List<StoryFlag> flagNeeded;
    [SerializeField] private List<StoryFlag> flagsForbidden;

    protected override bool CanPlay()
    {
        foreach (StoryFlag flag in flagNeeded)
        {
            if (!StoryManager.HasFlag(flag))
                return false;
        }

        foreach (StoryFlag flag in flagsForbidden)
        {
            if (StoryManager.HasFlag(flag))
                return false;
        }

        return true;
    }
}