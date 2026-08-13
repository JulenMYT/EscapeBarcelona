using System;
using UnityEngine;

public class MapLabelGameManager : MiniGame
{
    [SerializeField] private MapLabelAnchor[] slots;
    [SerializeField] private MapLabel[] labels;

    private bool isCompleted;

    public void CheckCompletion()
    {
        if (isCompleted)
            return;

        foreach (MapLabelAnchor slot in slots)
        {
            if (!slot.IsCorrect())
                return;
        }

        CompleteGame();
    }

    private void CompleteGame()
    {
        isCompleted = true;

        foreach (MapLabel label in labels)
            label.SetCanDrag(false);

        TriggerGameCompleted();
    }
}