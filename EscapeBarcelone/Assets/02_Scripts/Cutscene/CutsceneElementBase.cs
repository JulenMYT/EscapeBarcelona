using System.Collections;
using UnityEngine;

public class CutsceneElementBase : MonoBehaviour
{
    public float duration;
    public CutsceneHandler cutsceneHandler { get; private set; }

    // Awake (et non Start) : le handler est connu même si la cutscene démarre très tôt.
    // GetComponentInParent : l'élément peut être sur le handler ou sur un de ses enfants.
    private void Awake()
    {
        cutsceneHandler = GetComponentInParent<CutsceneHandler>();
    }

    public virtual void Execute()
    {

    }

    protected IEnumerator WaitAndAdvance()
    {
        yield return new WaitForSeconds(duration);
        cutsceneHandler.PlayNextElement();
    }
}
