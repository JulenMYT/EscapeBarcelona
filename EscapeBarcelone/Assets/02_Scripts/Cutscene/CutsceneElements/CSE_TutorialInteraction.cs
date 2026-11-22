using System.Collections;
using UnityEngine;

public class CSE_TutorialInteraction : CSE_Tutorial
{
    [SerializeField] private Interaction target;

    // Cutscene interrompue (salle déchargée) : on ne laisse pas d'abonnement vers un objet détruit.
    private void OnDestroy()
    {
        Cleanup();
    }

    public override void Execute()
    {
        cutsceneHandler.tutorialHandler.OnInteractionRequired += OnInteractionRequired;

        base.Execute();
    }

    protected override void OnTutorialComplete()
    {
        Cleanup();
        base.OnTutorialComplete();
    }

    private void OnInteractionRequired()
    {
        if (target == null)
            return;

        StartCoroutine(AllowTargetNextFrame());
    }

    private IEnumerator AllowTargetNextFrame()
    {
        yield return null;

        ServiceLocator.Get<InteractionManager>()
            .AllowObject(target.gameObject);

        target.OnClick += OnTargetClicked;
        target.Highlight();
    }

    private void OnTargetClicked()
    {
        target.OnClick -= OnTargetClicked;

        ServiceLocator.Get<InteractionManager>()
            .DisallowObject(target.gameObject);

        target.RemoveHighlight();

        cutsceneHandler.tutorialHandler.CompleteInteraction();
    }

    private void Cleanup()
    {
        TutorialHandler tutorialHandler = ServiceLocator.Get<TutorialHandler>();

        if (tutorialHandler != null)
            tutorialHandler.OnInteractionRequired -= OnInteractionRequired;

        if (target == null)
            return;

        target.OnClick -= OnTargetClicked;
        ServiceLocator.Get<InteractionManager>()?.DisallowObject(target.gameObject);
        target.RemoveHighlight();
    }
}