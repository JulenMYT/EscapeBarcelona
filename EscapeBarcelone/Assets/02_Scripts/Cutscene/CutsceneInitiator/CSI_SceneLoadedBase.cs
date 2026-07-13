using UnityEngine;

public abstract class CSI_SceneLoadedBase : CutsceneInitiator
{
    [SerializeField] protected SceneName sceneName;

    private SceneLoader sceneLoader;

    protected override void Initialize()
    {
        base.Initialize();

        sceneLoader = FindAnyObjectByType<SceneLoader>();
        sceneLoader.OnSceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(SceneName loadedScene)
    {
        if (loadedScene != sceneName)
            return;

        if (!CanPlay())
            return;

        if (playOnce)
            sceneLoader.OnSceneLoaded -= OnSceneLoaded;

        StartCutscene();
    }

    protected abstract bool CanPlay();

    private void OnDestroy()
    {
        if (sceneLoader != null)
            sceneLoader.OnSceneLoaded -= OnSceneLoaded;
    }
}