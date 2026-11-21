using UnityEngine;

public class RoomService : MonoBehaviour
{
    public AudioData roomMusic;

    private void Awake()
    {
        ServiceLocator.Register<RoomService>(this);
    }
}
