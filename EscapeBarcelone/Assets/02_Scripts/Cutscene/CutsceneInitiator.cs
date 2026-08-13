using UnityEngine;

public class CutsceneInitiator : MonoBehaviour
{
    private CutsceneHandler cutsceneHandler;

    private void Awake()
    {
        cutsceneHandler = GetComponent<CutsceneHandler>();

        Initialize();
    }

    protected virtual void Initialize() { }

    public void StartCutscene()
    {
        cutsceneHandler.Restart();
        cutsceneHandler.PlayNextElement();
        ServiceLocator.Get<InteractionManager>().Lock();
    }
}