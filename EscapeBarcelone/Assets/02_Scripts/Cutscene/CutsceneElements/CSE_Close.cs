using System.Collections.Generic;
using UnityEngine;

public class CSE_Close : CutsceneElementBase
{
    public bool closeAll = true;

    public override void Execute()
    {
        base.Execute();

        if (closeAll)
            ServiceLocator.Get<InspectionHandler>().CloseAll();
        else
            ServiceLocator.Get<InspectionHandler>().Close();

        cutsceneHandler.PlayNextElement();
    }
}
