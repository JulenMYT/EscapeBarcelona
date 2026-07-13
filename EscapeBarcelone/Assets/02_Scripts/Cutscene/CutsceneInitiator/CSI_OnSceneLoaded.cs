using UnityEngine;

public class CSI_OnSceneLoaded : CutsceneInitiator
{
    [SerializeField] private SceneName sceneName;
    private SceneLoader sceneLoader;

    private void OnSceneLoaded(SceneName loadedScene)
    {
        if (loadedScene == sceneName)
        {
            StartCutscene();
        }
    }

    protected override void Initialize()
    {
        base.Initialize();

        sceneLoader = FindAnyObjectByType<SceneLoader>();
        sceneLoader.OnSceneLoaded += OnSceneLoaded;
        
    }

    private void OnDestroy()
    {
        if (sceneLoader != null)
        {
            sceneLoader.OnSceneLoaded -= OnSceneLoaded;
        }
    }
}
