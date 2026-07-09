using System.Collections.Generic;
using UnityEngine;

public class CSE_Close : CutsceneElementBase
{
    [SerializeField] private List<InteractionElementBase> interactions;

    public override void Execute()
    {
        base.Execute();
        foreach (InteractionElementBase interaction in interactions)
        {
            interaction.Close();
        }

        cutsceneHandler.PlayNextElement();
    }
}
