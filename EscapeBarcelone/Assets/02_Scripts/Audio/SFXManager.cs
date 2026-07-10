using UnityEngine;
using UnityEngine.Audio;

public class SFXManager
{
    private readonly AudioSource[] audioSourcePool;

    public SFXManager(Transform parent, AudioMixerGroup mixerGroup, int poolSize)
    {
        audioSourcePool = new AudioSource[poolSize];

        for (int i = 0; i < poolSize; i++)
        {
            GameObject audioObject = new GameObject($"SFX_{i}");
            audioObject.transform.SetParent(parent);

            AudioSource source = audioObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.outputAudioMixerGroup = mixerGroup;

            audioSourcePool[i] = source;
        }
    }

    public AudioSource Play(
        AudioClip clip,
        float volume = 1f,
        float pitch = 1f,
        bool loop = false,
        float spatialBlend = 0f,
        int priority = 128)
    {
        AudioSource source = GetAvailableAudioSource();

        if (!source)
            return null;

        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.loop = loop;
        source.spatialBlend = spatialBlend;
        source.priority = priority;

        source.Play();

        return source;
    }

    public void Stop(AudioSource source)
    {
        if (source)
            source.Stop();
    }

    private AudioSource GetAvailableAudioSource()
    {
        foreach (AudioSource source in audioSourcePool)
        {
            if (!source.isPlaying)
                return source;
        }

        return null;
    }
}