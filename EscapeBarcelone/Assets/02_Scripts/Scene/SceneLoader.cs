using System;
using UnityEngine;

public enum SceneName
{
    Onett,
    Twoson,
    Threed,
    Fourside
}

public class SceneLoader : MonoBehaviour
{
    public event Action<SceneName> OnSceneLoaded;

    [SerializeField] private Scenes scenes;
    private Camera mainCamera;
    private FadePanel fadePanel;
    [SerializeField] private float fadeDuration = 0.5f;

    private Scenes.SceneData targetScene;

    private void Start()
    {
        mainCamera = Camera.main;
        fadePanel = FindAnyObjectByType<FadePanel>();
    }

    public void LoadScene(SceneName sceneName)
    {
        foreach (Scenes.SceneData scene in scenes.scenes)
        {
            if (scene.name == sceneName)
            {
                targetScene = scene;
                InteractionManager.SetBlocked(true);
                fadePanel.OnFadeInComplete += ChangeScene;
                fadePanel.FadeIn(fadeDuration);
                return;
            }
        }
    }

    private void ChangeScene()
    {
        InteractionManager.SetBlocked(false);

        fadePanel.OnFadeInComplete -= ChangeScene;

        mainCamera.transform.position = new Vector3(
            targetScene.cameraPosition.x,
            targetScene.cameraPosition.y,
            mainCamera.transform.position.z
        );

        OnSceneLoaded?.Invoke(targetScene.name);

        fadePanel.FadeOut(fadeDuration);
    }
}