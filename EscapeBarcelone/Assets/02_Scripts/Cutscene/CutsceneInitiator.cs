using UnityEngine;

public class CutsceneInitiator : MonoBehaviour
{
    private CutsceneHandler cutsceneHandler;

    private void Start()
    {
        cutsceneHandler = GetComponent<CutsceneHandler>();
    }
}
