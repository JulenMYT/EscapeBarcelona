using UnityEngine;

public static class InteractionManager
{
    public static bool Blocked { get; private set; }

    public static void SetBlocked(bool value)
    {
        Blocked = value;
        Debug.Log(value);
    }
}