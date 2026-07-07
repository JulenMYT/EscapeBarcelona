using UnityEngine;

public class CSE_LoadScene : CutsceneElementBase
{
    [SerializeField] private string sceneName;

    public override void Execute()
    {
        base.Execute();
        LoadScene();
    }

    private void LoadScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
