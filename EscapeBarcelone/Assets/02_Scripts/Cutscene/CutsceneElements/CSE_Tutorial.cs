using System.Collections;
using UnityEngine;

public class CSE_Tutorial : CutsceneElementBase
{
    [SerializeField] private Tutorial tutorial;
    [SerializeField] private Interaction target;

    public override void Execute()
    {
        cutsceneHandler.tutorialHandler.OnInteractionRequired += OnInteractionRequired;
        cutsceneHandler.tutorialHandler.OnSequenceComplete += OnTutorialComplete;

        cutsceneHandler.tutorialHandler.StartTutorial(tutorial);
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

    private void OnTutorialComplete()
    {
        if (target != null)
        {
            target.OnClick -= OnTargetClicked;

            ServiceLocator.Get<InteractionManager>()
                .DisallowObject(target.gameObject);
        }

        cutsceneHandler.tutorialHandler.OnInteractionRequired -= OnInteractionRequired;
        cutsceneHandler.tutorialHandler.OnSequenceComplete -= OnTutorialComplete;

        cutsceneHandler.PlayNextElement();
    }
}