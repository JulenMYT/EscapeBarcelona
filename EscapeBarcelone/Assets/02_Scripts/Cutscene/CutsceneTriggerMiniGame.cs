using UnityEngine;

public class CutsceneTriggerMiniGame : MonoBehaviour
{
    [SerializeField] private MiniGame miniGame;
    [SerializeField] private CutsceneInitiator cutscene;

    private void OnEnable()
    {
        miniGame.OnGameCompleted += OnGameCompleted;
    }

    private void OnDisable()
    {
        miniGame.OnGameCompleted -= OnGameCompleted;
    }

    private void OnGameCompleted()
    {
        cutscene.StartCutscene();
    }
}