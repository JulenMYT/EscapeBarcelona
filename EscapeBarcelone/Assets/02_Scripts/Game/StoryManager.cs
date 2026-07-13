using System.Collections.Generic;

public static class StoryManager
{
    private static readonly HashSet<StoryFlag> flags = new();

    public static bool HasFlag(StoryFlag flag)
    {
        return flags.Contains(flag);
    }

    public static void SetFlag(StoryFlag flag)
    {
        flags.Add(flag);
    }

    public static void RemoveFlag(StoryFlag flag)
    {
        flags.Remove(flag);
    }
}