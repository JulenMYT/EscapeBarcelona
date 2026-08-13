using UnityEngine;

public class MusicProxy : MonoBehaviour
{
    [SerializeField] private AudioData sound;

    public void PlayMusic() => ServiceLocator.Get<AudioManager>().PlayMusic(sound);
    public void StopMusic() => ServiceLocator.Get<AudioManager>().PlayMusic(null);
}