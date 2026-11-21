using System;
using UnityEngine;

public class MiniGame : MonoBehaviour
{
    public event Action OnGameCompleted;

    [SerializeField] private PersistentGuid guid;

    [Tooltip("Le joueur ne peut pas fermer le gros plan (croix, clic à côté, Échap, clic droit) " +
             "tant que ce mini-jeu n'est pas réussi. Utile pour un mini-jeu tutoriel.")]
    [SerializeField] private bool preventCloseUntilCompleted;

    public bool IsCompleted =>
        ServiceLocator.Get<WorldState>()?.completedMiniGame.Contains(guid.Guid) ?? false;

    // Le mini-jeu est dans un gros plan : il est actif exactement quand le gros plan est ouvert.
    protected virtual void OnEnable()
    {
        if (preventCloseUntilCompleted && !IsCompleted)
            ServiceLocator.Get<InspectionHandler>()?.AddCloseBlocker(this, gameObject);
    }

    protected virtual void OnDisable()
    {
        ServiceLocator.Get<InspectionHandler>()?.RemoveCloseBlocker(this);
    }

    private void Start()
    {
        if (IsCompleted)
        {
            SolveMiniGame();
            return;
        }

        Initialize();
    }

    protected void TriggerGameCompleted()
    {
        ServiceLocator.Get<WorldState>().completedMiniGame.Add(guid.Guid);

        // Réussi : le joueur peut de nouveau fermer.
        ServiceLocator.Get<InspectionHandler>()?.RemoveCloseBlocker(this);

        OnGameCompleted?.Invoke();
    }

    protected virtual void Initialize()
    {
    }

    protected virtual void SolveMiniGame()
    {
    }
}
