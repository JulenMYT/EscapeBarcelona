using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private int poolSize = 10;

    private List<AudioSource> sfxPool;
    private Coroutine musicRoutine;

    private void Awake()
    {
        ServiceLocator.Register<AudioManager>(this);
        sfxPool = new List<AudioSource>();

        for (int i = 0; i < poolSize; i++)
            sfxPool.Add(gameObject.AddComponent<AudioSource>());
    }

    public void PlaySFX(AudioData data, float position = 0)
    {
        for (int i = 0; i < poolSize; i++)
        {
            if (!sfxPool[i].isPlaying)
            {
                sfxPool[i].outputAudioMixerGroup = data.mixerGroup;
                sfxPool[i].clip = data.soundClip;
                sfxPool[i].volume = data.volume;
                sfxPool[i].time = position;
                sfxPool[i].PlayOneShot(data.soundClip, data.volume);

                return;
            }
        }
    }

    public void PlayMusic(AudioData data, float fadeTime = 0.5f, float position = 0)
    {
        if (musicSource.clip == data.soundClip)
        {
            if (musicRoutine != null)
            {
                StopCoroutine(musicRoutine);
                musicRoutine = null;
            }

            musicSource.outputAudioMixerGroup = data.mixerGroup;
            musicSource.loop = data.loop;

            if (!musicSource.isPlaying)
                musicSource.Play();

            if (musicSource.volume != data.volume)
                musicRoutine = StartCoroutine(Fade(data.volume, fadeTime));

            return;
        }

        if (musicRoutine != null)
            StopCoroutine(musicRoutine);

        musicRoutine = StartCoroutine(PlayMusicRoutine(data.soundClip, data.volume, fadeTime, data.mixerGroup, position));
    }

    private IEnumerator PlayMusicRoutine(AudioClip clip, float volume, float fadeTime, AudioMixerGroup mixerGroup, float position)
    {
        yield return Fade(0, fadeTime);

        musicSource.clip = clip;
        musicSource.volume = volume;
        musicSource.outputAudioMixerGroup = mixerGroup;
        musicSource.time = position;
        musicSource.Play();

        yield return Fade(volume, fadeTime);
    }

    private IEnumerator Fade(float target, float duration)
    {
        float start = musicSource.volume;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(start, target, time / duration);
            yield return null;
        }

        musicSource.volume = target;
    }

    public void StopMusic(float fadeTime = 0.5f)
    {
        StopCurrentMusicRoutine();
        musicRoutine = StartCoroutine(StopMusicRoutine(fadeTime));
    }

    private IEnumerator StopMusicRoutine(float fadeTime)
    {
        yield return Fade(0, fadeTime);

        musicSource.Stop();
        musicSource.clip = null;
    }

    private void StopCurrentMusicRoutine()
    {
        if (musicRoutine != null)
        {
            StopCoroutine(musicRoutine);
            musicRoutine = null;
        }
    }

    private IEnumerator FadeMixer(AudioMixer mixer, string parameter, float target, float duration)
    {
        mixer.GetFloat(parameter, out float start);

        float targetDb = Mathf.Log10(Mathf.Max(target, 0.0001f)) * 20f;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float value = Mathf.Lerp(start, targetDb, time / duration);
            mixer.SetFloat(parameter, value);
            yield return null;
        }

        mixer.SetFloat(parameter, targetDb);
    }

    public float GetMusicTime() => musicSource.time;
}

[System.Serializable]
public class AudioData
{
    public AudioClip soundClip;
    [Range(0, 1)] public float volume = 0.5f;
    public bool loop = false;
    public AudioMixerGroup mixerGroup;
}