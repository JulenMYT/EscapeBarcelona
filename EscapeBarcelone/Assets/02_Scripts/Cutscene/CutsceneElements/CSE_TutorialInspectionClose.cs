using UnityEngine;

public class CSE_TutorialInspectionClose : CSE_Tutorial
{
    [Tooltip("Gros plan dont on attend la fermeture. Vide = n'importe quelle fermeture.")]
    [SerializeField] private GameObject expectedView;

    // Cutscene interrompue (salle déchargée) : on ne laisse pas d'abonnement vers un objet détruit.
    private void OnDestroy()
    {
        InspectionHandler inspectionHandler = ServiceLocator.Get<InspectionHandler>();

        if (inspectionHandler != null)
            inspectionHandler.OnInspectionClosed -= OnInspectionClosed;
    }

    public override void Execute()
    {
        ServiceLocator.Get<InspectionHandler>()
            .OnInspectionClosed += OnInspectionClosed;

        base.Execute();
    }

    protected override void OnTutorialComplete()
    {
        ServiceLocator.Get<InspectionHandler>()
            .OnInspectionClosed -= OnInspectionClosed;

        base.OnTutorialComplete();
    }

    private void OnInspectionClosed(GameObject closedView)
    {
        if (expectedView != null && closedView != expectedView)
            return;

        ServiceLocator.Get<InspectionHandler>()
            .OnInspectionClosed -= OnInspectionClosed;

        cutsceneHandler.tutorialHandler.CompleteInteraction();
    }
}
