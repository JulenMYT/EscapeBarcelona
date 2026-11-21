using UnityEngine;

/// <summary>
/// État partagé d'une radio, identifiée par un nom (ex. "Radio"), stocké dans WorldState :
/// la radio elle-même et ses échos dans d'autres salles lisent le même état.
///
/// La musique « continue » même quand on n'est pas dans la salle : la position est
/// calculée depuis le moment où la radio a été allumée.
/// </summary>
public static class RadioState
{
    public static bool IsOn(string radioId)
    {
        WorldState worldState = ServiceLocator.Get<WorldState>();
        return worldState != null && worldState.activatedObjects.Contains(radioId);
    }

    public static void SetOn(string radioId, bool on)
    {
        WorldState worldState = ServiceLocator.Get<WorldState>();

        if (worldState == null)
            return;

        if (on)
        {
            worldState.activatedObjects.Add(radioId);

            if (!worldState.audioStartTimes.ContainsKey(radioId))
                worldState.audioStartTimes[radioId] = Time.timeAsDouble;
        }
        else
        {
            worldState.activatedObjects.Remove(radioId);
            worldState.audioStartTimes.Remove(radioId);
        }
    }

    /// <summary>Où en est la musique de cette radio (en secondes, bouclée sur la durée du clip).</summary>
    public static float GetPosition(string radioId, AudioClip clip)
    {
        WorldState worldState = ServiceLocator.Get<WorldState>();

        if (worldState == null || clip == null || clip.length <= 0f)
            return 0f;

        if (!worldState.audioStartTimes.TryGetValue(radioId, out double startTime))
            return 0f;

        return (float)((Time.timeAsDouble - startTime) % clip.length);
    }
}
