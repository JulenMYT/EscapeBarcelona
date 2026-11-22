using System;
using System.Collections.Generic;
using UnityEngine;

public class WorldState : MonoBehaviour
{
    public event Action<StoryFlag> OnStoryFlagAdded;
    public event Action<StoryFlag> OnStoryFlagRemoved;

    [Tooltip("ÉDITEUR UNIQUEMENT : flags ajoutés au lancement du Play, pour tester une salle " +
             "sans rejouer tout le jeu (ex. HasBook pour tester la Room 4). Ignoré dans le build.")]
    [SerializeField] private List<StoryFlag> editorTestFlags = new();

    //Save
    // Privé : on passe toujours par AddFlag/RemoveFlag pour que les événements soient déclenchés.
    private readonly HashSet<StoryFlag> storyFlags = new();

    public HashSet<string> activatedObjects = new();
    public HashSet<string> completedMiniGame = new();
    public HashSet<string> puzzlePiecesCollected = new();
    public PuzzleState puzzleState = new();
    public CrosswordState crosswordState = new();

    //No Save
    public Dictionary<string, double> audioStartTimes = new();

    /// <summary>Lecture seule : pour modifier, utiliser AddFlag / RemoveFlag.</summary>
    public IReadOnlyCollection<StoryFlag> StoryFlags => storyFlags;

    private void Awake()
    {
        ServiceLocator.Register<WorldState>(this);

        if (Application.isEditor)
        {
            foreach (StoryFlag flag in editorTestFlags)
                AddFlag(flag);
        }
    }

    public bool HasFlag(StoryFlag flag)
    {
        return storyFlags.Contains(flag);
    }

    public void AddFlag(StoryFlag flag)
    {
        if (flag == StoryFlag.None)
        {
            Debug.LogWarning("WorldState : tentative d'ajouter StoryFlag.None (flag oublié dans l'inspecteur ?).", this);
            return;
        }

        if (!storyFlags.Add(flag))
            return;

        OnStoryFlagAdded?.Invoke(flag);
    }

    public void RemoveFlag(StoryFlag flag)
    {
        if (!storyFlags.Remove(flag))
            return;

        OnStoryFlagRemoved?.Invoke(flag);
    }
}
