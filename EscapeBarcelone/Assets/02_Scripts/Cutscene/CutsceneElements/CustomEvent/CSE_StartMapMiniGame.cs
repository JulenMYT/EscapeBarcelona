using UnityEngine;

public class CSE_CustomEvent : CutsceneElementBase
{
    [SerializeField] private MapMiniGame mapMiniGame;
    public override void Execute()
    {
        mapMiniGame.OpenMapMiniGame();
        cutsceneHandler.PlayNextElement();
    }
}
