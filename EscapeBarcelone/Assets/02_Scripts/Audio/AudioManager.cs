using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;

    public static AudioManager Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = new GameObject("AudioManager").AddComponent<AudioManager>();
                DontDestroyOnLoad(_instance.gameObject);
            }

            return _instance;
        }
    }

    public MusicManager Music { get; private set; }
    public SFXManager SFX { get; private set; }

    private AudioMixer audioMixer;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        _ = Instance;
    }

    private void Awake()
    {
        audioMixer = Resources.Load<AudioMixer>("MainMixer");

        AudioMixerGroup musicGroup = audioMixer.FindMatchingGroups("Music")[0];

        AudioMixerGroup sfxDefaultGroup = audioMixer.FindMatchingGroups("SFX/Default")[0];
        AudioMixerGroup sfxRadioGroup = audioMixer.FindMatchingGroups("SFX/Radio")[0];

        Music = new MusicManager(transform, musicGroup);
        SFX = new SFXManager(transform, sfxDefaultGroup, sfxRadioGroup, 10);
    }

    private const string MASTER_VOLUME = "MasterVolume";
    private const string MUSIC_VOLUME = "MusicVolume";
    private const string SFX_VOLUME = "SFXVolume";

    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat(MASTER_VOLUME, ConvertVolume(volume));
    }

    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat(MUSIC_VOLUME, ConvertVolume(volume));
    }

    public void SetSFXVolume(float volume)
    {
        audioMixer.SetFloat(SFX_VOLUME, ConvertVolume(volume));
    }

    private float ConvertVolume(float volume)
    {
        return Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;
    }
}