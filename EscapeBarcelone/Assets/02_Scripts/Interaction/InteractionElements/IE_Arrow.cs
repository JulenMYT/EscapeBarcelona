using UnityEngine;

public class IE_Arrow : InteractionElementBase
{
    [SerializeField] private SceneName sceneName;
    private static SceneLoader sceneLoader;

    public override void Initialize()
    {
        base.Initialize();
        if (sceneLoader == null)
        sceneLoader = FindAnyObjectByType<SceneLoader>();
    }

    public override void Interact()
    {
        base.Interact();

        Exit();
        
        sceneLoader.LoadScene(sceneName);
    }
}   