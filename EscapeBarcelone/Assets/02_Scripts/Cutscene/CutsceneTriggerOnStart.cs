using System.Collections;
using UnityEngine;

public class CutsceneTriggerOnStart : MonoBehaviour
{
    [SerializeField] private CutsceneInitiator cutscene;

    private void Start()
    {
        StartCoroutine(WaitAndPlayCutscene());
    }

    private IEnumerator WaitAndPlayCutscene()
    {
        yield return null;

        cutscene.StartCutscene();
    }
}