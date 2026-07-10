using UnityEngine;
using UnityEngine.Audio;

public class MusicManager
{
    private readonly AudioSource source;

    public MusicManager(Transform parent, AudioMixerGroup mixerGroup)
    {
        GameObject musicObject = new GameObject("Music");
        musicObject.transform.SetParent(parent);

        source = musicObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = true;
        source.outputAudioMixerGroup = mixerGroup;
    }

    public void Play(AudioClip clip, float volume = 1f)
    {
        source.clip = clip;
        source.volume = volume;
        source.Play();
    }

    public void Stop()
    {
        source.Stop();
    }

    public void Pause()
    {
        source.Pause();
    }

    public void Resume()
    {
        source.UnPause();
    }

    public void SetVolume(float volume)
    {
        source.volume = volume;
    }
}