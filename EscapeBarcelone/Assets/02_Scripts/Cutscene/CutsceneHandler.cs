using UnityEngine;

/// <summary>
/// Joue dans l'ordre les éléments (CSE_…) placés sur ce GameObject et ses enfants.
/// Le lancement, la file d'attente et le blocage des entrées sont gérés par CutsceneDirector.
/// </summary>
public class CutsceneHandler : MonoBehaviour
{
    public Camera cam { get; private set; }
    public DialogueHandler dialogueHandler => ServiceLocator.Get<DialogueHandler>();
    public TutorialHandler tutorialHandler => ServiceLocator.Get<TutorialHandler>();

    public bool IsPlaying { get; private set; }

    private CutsceneElementBase[] cutsceneElements;
    private int index = -1;

    private void Awake()
    {
        cam = Camera.main;
        cutsceneElements = GetComponentsInChildren<CutsceneElementBase>();
    }

    // Salle déchargée en pleine cutscene (CSE_ChangeScene suivi d'autres éléments…) :
    // on prévient le directeur pour que le jeu ne reste pas bloqué.
    private void OnDestroy()
    {
        if (!IsPlaying)
            return;

        IsPlaying = false;

        if (CutsceneDirector.IsPlaying)
            CutsceneDirector.Instance.NotifyFinished(this);
    }

    /// <summary>Demande à jouer cette cutscene (attend son tour si une autre est en cours).</summary>
    public void Play()
    {
        CutsceneDirector.Instance.Play(this);
    }

    /// <summary>Appelé par CutsceneDirector quand c'est le tour de cette cutscene.</summary>
    public void BeginPlayback()
    {
        // Coupe les attentes éventuelles d'une lecture précédente (CSE_Wait…).
        foreach (CutsceneElementBase element in cutsceneElements)
            element.StopAllCoroutines();

        index = -1;
        IsPlaying = true;

        PlayNextElement();
    }

    public void PlayNextElement()
    {
        // Rappel tardif d'un élément alors que la cutscene est finie : ignoré.
        if (!IsPlaying)
            return;

        index++;

        if (index < cutsceneElements.Length)
        {
            cutsceneElements[index].Execute();
            return;
        }

        Finish();
    }

    private void Finish()
    {
        IsPlaying = false;
        CutsceneDirector.Instance?.NotifyFinished(this);
    }
}
