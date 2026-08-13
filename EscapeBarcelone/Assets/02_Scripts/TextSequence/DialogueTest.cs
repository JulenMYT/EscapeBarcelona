using UnityEngine;

public class DialogueTest : MonoBehaviour
{
    public Dialogue dialogue;

    void Start()
    {
        ServiceLocator.Get<DialogueHandler>().StartDialogue(dialogue);
    }
}
