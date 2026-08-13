using UnityEngine;

public class Arrow : Interaction
{
    [SerializeField] private string sceneName;

    protected override void Interact()
    {
        ServiceLocator.Get<RoomTransitionManager>().EnterRoom(sceneName);
    }
}