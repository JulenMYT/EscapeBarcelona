using UnityEngine;

public class FirstSceneEvent : MonoBehaviour
{
    [SerializeField] private TextSequenceViewer textSequenceViewer;
    [SerializeField] private Highlight treeHighlight;
    [SerializeField] private Highlight pooHighlight;
    [SerializeField] private ClickableUI pooClickable;

    private void OnEnable()
    {
        textSequenceViewer.OnTextEvent += HandleTextEvent;
        pooClickable.OnClicked += () => textSequenceViewer.ContinueAfterExternalAction();
    }

    private void OnDisable()
    {
        textSequenceViewer.OnTextEvent -= HandleTextEvent;
    }

    private void HandleTextEvent(string eventKey)
    {
        switch (eventKey)
        {
            case "HighlightTree":
                treeHighlight.StartHighlight();
                break;

            case "StopHighlightTree":
                treeHighlight.StopHighlight();
                break;

            case "HighlightPoo":
                treeHighlight.StopHighlight();
                pooHighlight.StartHighlight();
                pooClickable.EnableClick();
                break;

            case "StopHighlightPoo":
                pooHighlight.StopHighlight();
                pooClickable.DisableClick();
                break;
        }
    }
}