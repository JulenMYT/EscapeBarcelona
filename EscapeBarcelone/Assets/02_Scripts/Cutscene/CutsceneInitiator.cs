using UnityEngine;

public class CutsceneInitiator : MonoBehaviour
{
    [SerializeField] protected bool playOnce = true;

    private CutsceneHandler cutsceneHandler;

    private bool played = false;

    private void Start()
    {
        cutsceneHandler = GetComponent<CutsceneHandler>();

        Initialize();
    }

    protected virtual void Initialize() { }

    public void StartCutscene()
    {
        if (playOnce && played)
            return;

        played = true;
        cutsceneHandler.Restart();
        cutsceneHandler.PlayNextElement();
        InteractionManager.SetBlocked(true);
    }
}