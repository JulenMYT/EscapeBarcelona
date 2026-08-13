using UnityEngine;

public class CutsceneTriggerMiniGame : MonoBehaviour
{
    [SerializeField] private MiniGame miniGame;
    [SerializeField] protected CutsceneInitiator cutscene;

    private void OnEnable()
    {
        miniGame.OnGameCompleted += OnGameCompleted;
    }

    private void OnDisable()
    {
        miniGame.OnGameCompleted -= OnGameCompleted;
    }

    protected virtual void OnGameCompleted()
    {
        cutscene.StartCutscene();
    }
}