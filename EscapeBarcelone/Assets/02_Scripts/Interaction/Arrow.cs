using UnityEngine;

public class Arrow : Interaction
{
    [SerializeField] private string sceneName;

    protected override void OnMouseDown()
    {
        if (!ServiceLocator.Get<InteractionManager>().CanInteract(gameObject))
            return;

        OnClick?.Invoke();

        ServiceLocator.Get<RoomTransitionManager>().EnterRoom(sceneName);
    }
}