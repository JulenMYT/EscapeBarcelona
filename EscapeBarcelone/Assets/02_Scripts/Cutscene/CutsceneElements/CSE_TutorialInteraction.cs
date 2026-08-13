using System.Collections;
using UnityEngine;

public class CSE_TutorialInteraction : CSE_Tutorial
{
    [SerializeField] private Interaction target;

    public override void Execute()
    {
        cutsceneHandler.tutorialHandler.OnInteractionRequired += OnInteractionRequired;

        base.Execute();
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

    protected override void OnTutorialComplete()
    {
        target.OnClick -= OnTargetClicked;

        ServiceLocator.Get<InteractionManager>()
            .DisallowObject(target.gameObject);

        target.RemoveHighlight();

        cutsceneHandler.tutorialHandler.OnInteractionRequired -= OnInteractionRequired;

        base.OnTutorialComplete();
    }
}