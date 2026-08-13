using System.Collections.Generic;
using UnityEngine;

public class WorldState : MonoBehaviour
{
    public HashSet<StoryFlag> storyFlags = new();

    private void Awake() => ServiceLocator.Register<WorldState>(this);
}
