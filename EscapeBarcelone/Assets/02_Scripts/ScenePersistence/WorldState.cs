using System.Collections.Generic;
using UnityEngine;

public class WorldState : MonoBehaviour
{
    //Save
    public HashSet<StoryFlag> storyFlags = new();

    public HashSet<string> activatedObjects = new();
    public HashSet<string> completedMiniGame = new();
    public HashSet<string> puzzlePiecesCollected = new();
    public PuzzleState puzzleState = new();

    //No Save
    public Dictionary<string, double> audioStartTimes = new();

    private void Awake()
    {
        ServiceLocator.Register<WorldState>(this);
    }
}