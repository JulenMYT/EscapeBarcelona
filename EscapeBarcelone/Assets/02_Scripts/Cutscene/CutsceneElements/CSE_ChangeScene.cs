using UnityEngine;

public class CSE_ChangeScene : CutsceneElementBase
{
    [SerializeField] private string sceneName;

    public override void Execute()
    {
        ServiceLocator.Get<RoomTransitionManager>().EnterRoom(sceneName);

        cutsceneHandler.PlayNextElement();
    }
}
