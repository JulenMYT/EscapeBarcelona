using UnityEngine;

public class CutsceneInitiator : MonoBehaviour
{
    private CutsceneHandler cutsceneHandler;
    [SerializeField] private bool playOnStart = true;

    private void Start()
    {
        cutsceneHandler = GetComponent<CutsceneHandler>();
    }

    private void Update()
    {
        if (playOnStart)
        {
            StartCutscene();
            playOnStart = false;
        }
    }

    public void StartCutscene()
    {
        cutsceneHandler.PlayNextElement();
    }
}
