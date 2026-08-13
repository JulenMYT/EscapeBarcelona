using System.Collections;
using UnityEngine;

public class Radio : Interaction
{
    [SerializeField] private AudioData sound;

    private PersistentGuid persistentGuid;
    private WorldState worldState;

    protected override void Initialize()
    {
        persistentGuid = GetComponent<PersistentGuid>();
    }

    private void Start()
    {
        StartCoroutine(DelayedStart());
    }

    private IEnumerator DelayedStart()
    {
        yield return null;

        worldState = ServiceLocator.Get<WorldState>();

        if (worldState.activatedObjects.Contains(persistentGuid.Guid))
            PlayMusic();
    }

    protected override void Interact()
    {
        ToggleRadio();
    }

    public void ToggleRadio()
    {
        if (worldState.activatedObjects.Contains(persistentGuid.Guid))
        {
            worldState.activatedObjects.Remove(persistentGuid.Guid);
            StopMusic();
        }
        else
        {
            worldState.activatedObjects.Add(persistentGuid.Guid);
            PlayMusic();
        }
    }

    public void PlayMusic()
    {
        double startTime;

        if (!worldState.audioStartTimes.TryGetValue(persistentGuid.Guid, out startTime))
        {
            startTime = Time.timeAsDouble;
            worldState.audioStartTimes[persistentGuid.Guid] = startTime;
        }

        float position = (float)(Time.timeAsDouble - startTime);
        position %= sound.soundClip.length;

        ServiceLocator.Get<AudioManager>().PlayMusic(sound, position: position);
    }

    public void StopMusic()
    {
        worldState.audioStartTimes.Remove(persistentGuid.Guid);
        ServiceLocator.Get<AudioManager>().StopMusic();
    }
}