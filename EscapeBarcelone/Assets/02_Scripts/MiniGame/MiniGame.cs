using System;
using UnityEngine;

public class MiniGame : MonoBehaviour
{
    public event Action OnGameCompleted;

    [SerializeField] private PersistentGuid guid;

    private void Start()
    {
        if (ServiceLocator.Get<WorldState>().completedMiniGame.Contains(guid.Guid))
            SolveMiniGame();
    }

    protected void TriggerGameCompleted()
    {
        ServiceLocator.Get<WorldState>().completedMiniGame.Add(guid.Guid);
        OnGameCompleted?.Invoke();
    }

    protected virtual void SolveMiniGame()
    {
    }
}