using System;
using System.Collections.Generic;

/// <summary>
/// Condition basée sur les StoryFlags : tous les flags requis doivent être présents
/// et aucun flag interdit ne doit l'être. Une condition vide est toujours remplie.
/// </summary>
[Serializable]
public class StoryCondition
{
    public List<StoryFlag> requiredFlags = new();
    public List<StoryFlag> forbiddenFlags = new();

    public bool IsEmpty =>
        (requiredFlags == null || requiredFlags.Count == 0) &&
        (forbiddenFlags == null || forbiddenFlags.Count == 0);

    public bool IsMet()
    {
        if (IsEmpty)
            return true;

        return Check(requiredFlags, forbiddenFlags);
    }

    public static bool Check(IEnumerable<StoryFlag> requiredFlags, IEnumerable<StoryFlag> forbiddenFlags)
    {
        WorldState worldState = ServiceLocator.Get<WorldState>();

        if (worldState == null)
            return false;

        if (requiredFlags != null)
        {
            foreach (StoryFlag flag in requiredFlags)
            {
                if (!worldState.HasFlag(flag))
                    return false;
            }
        }

        if (forbiddenFlags != null)
        {
            foreach (StoryFlag flag in forbiddenFlags)
            {
                if (worldState.HasFlag(flag))
                    return false;
            }
        }

        return true;
    }
}
