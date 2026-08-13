using System.Collections.Generic;
using UnityEngine;

public class WorldState : MonoBehaviour
{
    public HashSet<StoryFlag> storyFlags = new();

    public HashSet<string> activatedObjects = new();
    public Dictionary<string, double> audioStartTimes = new();

    public HashSet<string> completedMiniGame = new();

    private void Awake()
    {
        ServiceLocator.Register<WorldState>(this);
    }
}