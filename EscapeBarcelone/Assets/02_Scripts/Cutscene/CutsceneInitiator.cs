using UnityEngine;

public class CutsceneInitiator : MonoBehaviour
{
    private CutsceneHandler cutsceneHandler;
    private SceneLoader sceneLoader;

    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool playOnSceneLoaded = false;
    [SerializeField] private SceneName sceneName;
    [SerializeField] private bool playOnce = true;

    private bool played = false;

    private void Start()
    {
        cutsceneHandler = GetComponent<CutsceneHandler>();

        if (playOnSceneLoaded)
        {
            sceneLoader = FindAnyObjectByType<SceneLoader>();
            sceneLoader.OnSceneLoaded += OnSceneLoaded;
        }
    }

    private void Update()
    {
        if (playOnStart)
        {
            StartCutscene();
            playOnStart = false;
        }
    }

    private void OnSceneLoaded(SceneName loadedScene)
    {
        if (loadedScene == sceneName)
        {
            StartCutscene();
        }
    }

    public void StartCutscene()
    {
        if (playOnce && played)
            return;

        played = true;
        cutsceneHandler.Restart();
        cutsceneHandler.PlayNextElement();
        InteractionManager.SetBlocked(true);
    }

    private void OnDestroy()
    {
        if (sceneLoader != null)
        {
            sceneLoader.OnSceneLoaded -= OnSceneLoaded;
        }
    }
}