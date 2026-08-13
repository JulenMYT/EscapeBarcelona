using UnityEngine;

public class IE_Radio : MonoBehaviour
{
    [SerializeField] private MusicProxy sound;

    private bool activated;

    private void Start()
    {
        activated = false;
    }

    public void ToggleRadio()
    {
        activated = !activated;

        if (activated)
            sound.PlayMusic();
        else
            sound.StopMusic();
    }
}
