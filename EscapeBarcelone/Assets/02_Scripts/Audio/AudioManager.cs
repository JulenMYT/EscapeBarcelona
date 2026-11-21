using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

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

    public void PlaySFX(AudioData data)
    {
        if (data == null || data.soundClip == null)
            return;

        for (int i = 0; i < poolSize; i++)
        {
            if (!sfxPool[i].isPlaying)
            {
                sfxPool[i].outputAudioMixerGroup = data.mixerGroup;
                sfxPool[i].PlayOneShot(data.soundClip, data.volume);

                return;
            }
        }
    }

    public void PlayMusic(AudioData data, float fadeTime = 0.5f, float position = 0)
    {
        // Pas de musique demandée : on coupe proprement (au lieu de jouer un clip vide).
        if (data == null || data.soundClip == null)
        {
            if (musicSource.clip != null)
                StopMusic(fadeTime);

            return;
        }

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

        musicRoutine = StartCoroutine(PlayMusicRoutine(data.soundClip, data.volume, fadeTime, data.mixerGroup, position, data.loop));
    }

    private IEnumerator PlayMusicRoutine(AudioClip clip, float volume, float fadeTime, AudioMixerGroup mixerGroup, float position, bool loop)
    {
        yield return Fade(0, fadeTime);

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.volume = 0f;   // part de 0 pour que le fondu d'entrée s'entende vraiment
        musicSource.outputAudioMixerGroup = mixerGroup;
        // + fadeTime : le fondu de sortie a pris ce temps, la radio a « continué » pendant ce temps.
        musicSource.time = clip.length > 0f ? Mathf.Repeat(position + fadeTime, clip.length) : 0f;
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