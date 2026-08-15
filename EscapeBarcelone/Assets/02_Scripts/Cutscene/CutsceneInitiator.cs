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
        ServiceLocator.Get<InteractionManager>().Lock();
        ServiceLocator.Get<InspectionHandler>().SetCanClose(false);

        cutsceneHandler.Restart();
        cutsceneHandler.PlayNextElement();
    }
}