using System;
using UnityEngine;

public class MiniGame : MonoBehaviour
{
    public event Action OnGameCompleted;

    protected void TriggerGameCompleted()
    {
        OnGameCompleted?.Invoke();
    }
}