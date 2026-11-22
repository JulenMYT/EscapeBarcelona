using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// La boussole : le joueur sélectionne un lieu dans la grille (comme une recherche de mots)
/// pour s'y « téléporter ». Lieu reconnu → entrées bloquées, boussole fermée, dialogue
/// optionnel, puis changement de scène.
/// </summary>
public class WordHuntManager : MonoBehaviour
{
    [SerializeField] private WordHuntData data;
    [SerializeField] private WordHuntView view;

    [Tooltip("Temps pendant lequel la sélection reste affichée avant le départ (secondes).")]
    [SerializeField] private float delayBeforeTravel = 0.6f;

    private bool isTravelling;

    private void OnEnable()
    {
        view.SelectionEnded += HandleSelectionEnded;
    }

    private void Start()
    {
        view.CreateGrid(data);
    }

    private void OnDisable()
    {
        view.SelectionEnded -= HandleSelectionEnded;
    }

    private void HandleSelectionEnded(Vector2Int start, Vector2Int end, Vector2Int direction)
    {
        if (isTravelling)
            return;

        string selected = ReadWord(start, end, direction);
        CompassDestination destination = FindDestination(selected);

        if (destination == null)
        {
            view.HideSelection();
            return;
        }

        StartCoroutine(Travel(destination));
    }

    private string ReadWord(Vector2Int start, Vector2Int end, Vector2Int direction)
    {
        string word = string.Empty;
        Vector2Int position = start;

        while (position != end && direction != Vector2Int.zero)
        {
            word += data.Rows[position.y][position.x];
            position += direction;
        }

        word += data.Rows[position.y][position.x];
        return word;
    }

    /// <summary>Le mot sélectionné, lu dans un sens ou dans l'autre, sans accents ni casse.</summary>
    private CompassDestination FindDestination(string selected)
    {
        string forward = Normalize(selected);
        string backward = new string(forward.Reverse().ToArray());

        return data.Destinations.FirstOrDefault(d =>
        {
            string target = Normalize(d.word);
            return target.Length > 1 && (target == forward || target == backward);
        });
    }

    private static string Normalize(string text)
    {
        return new string((text ?? string.Empty).Where(c => !char.IsWhiteSpace(c)).Select(CrosswordText.Normalize).ToArray());
    }

    private IEnumerator Travel(CompassDestination destination)
    {
        isTravelling = true;

        // Plus aucune entrée du joueur, comme pendant une cutscene (sauf avancer le dialogue).
        CutsceneDirector director = CutsceneDirector.Instance;
        director.Hold(this);

        // On laisse le joueur voir sa sélection un instant.
        yield return new WaitForSecondsRealtime(delayBeforeTravel);

        bool allowed = destination.condition == null || destination.condition.IsMet();
        bool alreadyThere = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == destination.sceneName;

        if (!allowed || alreadyThere)
        {
            if (!allowed)
                yield return PlayDialogue(destination.lockedDialogue);

            view.HideSelection();
            Finish(director);
            yield break;
        }

        // La boussole se ferme avant le dialogue, pour qu'on voie la scène.
        // (Ce script est sur la boussole : on continue la suite depuis le directeur,
        // qui reste actif, sinon la coroutine s'arrêterait avec la boussole.)
        director.StartCoroutine(ContinueTravel(destination, director));
    }

    private IEnumerator ContinueTravel(CompassDestination destination, CutsceneDirector director)
    {
        ServiceLocator.Get<Compass>()?.CloseCompass();
        view.HideSelection();

        yield return PlayDialogue(destination.dialogueBeforeTravel);

        if (destination.flagOnTravel != StoryFlag.None)
            ServiceLocator.Get<WorldState>()?.AddFlag(destination.flagOnTravel);

        if (Application.CanStreamedLevelBeLoaded(destination.sceneName))
        {
            ServiceLocator.Get<RoomTransitionManager>()?.EnterRoom(destination.sceneName);
        }
        else
        {
            // Normal tant que la scène n'est pas créée : on prévient sans rien casser.
            Debug.LogWarning($"Boussole : la scène « {destination.sceneName} » (destination « {destination.word} ») " +
                             "n'existe pas ou n'est pas dans la liste des scènes du build.");
        }

        Finish(director);
    }

    private static IEnumerator PlayDialogue(Dialogue dialogue)
    {
        DialogueHandler dialogueHandler = ServiceLocator.Get<DialogueHandler>();

        if (dialogue == null || dialogueHandler == null)
            yield break;

        bool done = false;
        dialogueHandler.StartDialogue(dialogue, () => done = true);

        while (!done)
            yield return null;
    }

    private void Finish(CutsceneDirector director)
    {
        isTravelling = false;
        director.Release(this);
    }
}
