using UnityEngine;

public class CSE_Close : CutsceneElementBase
{
    [SerializeField] private InteractionElementBase interaction;

    public override void Execute()
    {
        base.Execute();

        interaction.Close();

        cutsceneHandler.PlayNextElement();
    }
}
