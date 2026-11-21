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

    /// <summary>
    /// Lance la cutscene. Si une autre est déjà en cours, celle-ci attend son tour.
    /// Le blocage des entrées est géré par CutsceneDirector.
    /// </summary>
    public void StartCutscene()
    {
        cutsceneHandler.Play();
    }
}
