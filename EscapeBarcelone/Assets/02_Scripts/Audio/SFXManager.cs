using UnityEngine;
using UnityEngine.Audio;

public enum AudioChannel
{
    Default,
    Radio,
    UI,
    Voice
}

public class SFXManager
{
    private readonly AudioSource[] audioSourcePool;
    private readonly AudioMixerGroup defaultMixer;
    private readonly AudioMixerGroup radioMixer;

    public SFXManager(
        Transform parent,
        AudioMixerGroup defaultMixer,
        AudioMixerGroup radioMixer,
        int poolSize)
    {
        this.defaultMixer = defaultMixer;
        this.radioMixer = radioMixer;

        audioSourcePool = new AudioSource[poolSize];

        for (int i = 0; i < poolSize; i++)
        {
            GameObject audioObject = new GameObject($"SFX_{i}");
            audioObject.transform.SetParent(parent);

            AudioSource source = audioObject.AddComponent<AudioSource>();
            source.playOnAwake = false;

            audioSourcePool[i] = source;
        }
    }

    public AudioSource Play(
        AudioClip clip,
        float volume = 1f,
        float pitch = 1f,
        bool loop = false,
        float spatialBlend = 0f,
        int priority = 128,
        AudioChannel type = AudioChannel.Default)
    {
        AudioSource source = GetAvailableAudioSource();

        if (!source)
            return null;

        source.outputAudioMixerGroup = type switch
        {
            AudioChannel.Radio => radioMixer,
            _ => defaultMixer
        };

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