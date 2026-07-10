using UnityEngine;

public class IE_Radio : InteractionElementBase
{
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private bool startActivate;
    [SerializeField] private SceneName sceneName;

    private AudioSource audioSource;
    private bool activated;

    private static SceneLoader sceneLoader;

    public override void Initialize()
    {
        base.Initialize();

        if (sceneLoader == null)
            sceneLoader = FindAnyObjectByType<SceneLoader>();

        sceneLoader.OnSceneLoaded += OnSceneLoaded;

        if (startActivate)
        {
            Activate();
        }
    }

    private void OnSceneLoaded(SceneName loadedScene)
    {
        if (loadedScene == sceneName)
        {
            if (startActivate)
                Activate();
        }
        else
        {
            if (activated)
                Deactivate();
        }
    }

    private void Activate()
    {
        if (activated)
            return;

        activated = true;
        audioSource = AudioManager.Instance.SFX.Play(audioClip, loop:true, type:AudioChannel.Radio);
    }

    private void Deactivate()
    {
        if (!activated)
            return;

        activated = false;

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource = null;
        }
    }

    public override void Interact()
    {
        if (activated)
        {
            Deactivate();
        }
        else
        {
            Activate();
        }
    }

    private void OnDestroy()
    {
        if (sceneLoader != null)
            sceneLoader.OnSceneLoaded -= OnSceneLoaded;
    }
}
