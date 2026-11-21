using UnityEngine;

public class CutsceneTriggerMiniGame : MonoBehaviour
{
    [SerializeField] protected MiniGame miniGame;
    [SerializeField] protected CutsceneInitiator cutscene;

    protected virtual void OnEnable()
    {
        miniGame.OnGameCompleted += OnGameCompleted;
    }

    protected virtual void OnDisable()
    {
        miniGame.OnGameCompleted -= OnGameCompleted;
    }

    protected virtual void OnGameCompleted()
    {
        cutscene.StartCutscene();
    }
}
