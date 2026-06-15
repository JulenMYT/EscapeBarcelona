using UnityEngine;

public class FirstSceneEventRouter : MonoBehaviour
{
    [SerializeField] private TextSequenceViewer textSequenceViewer;
    [SerializeField] private Highlight treeHighlight;
    [SerializeField] private Highlight pooHighlight;
    [SerializeField] private ClickableUI pooClickable;

    private void Awake()
    {
        pooClickable.DisableClick();
        treeHighlight.StopHighlight();
        pooHighlight.StopHighlight();
    }

    private void OnEnable()
    {
        textSequenceViewer.OnTextEvent += HandleTextEvent;
        pooClickable.OnClicked += HandlePooClicked;
    }

    private void OnDisable()
    {
        textSequenceViewer.OnTextEvent -= HandleTextEvent;
        pooClickable.OnClicked -= HandlePooClicked;
    }

    private void HandleTextEvent(string eventKey)
    {
        switch (eventKey)
        {
            case "HighlightTree":
                ShowTreeHighlight();
                break;

            case "HighlightPoo":
                ShowPooHighlight();
                break;
        }
    }

    private void ShowTreeHighlight()
    {
        treeHighlight.StartHighlight();
    }

    private void ShowPooHighlight()
    {
        treeHighlight.StopHighlight();
        pooHighlight.StartHighlight();
        pooClickable.EnableClick();
    }

    private void HandlePooClicked()
    {
        pooClickable.DisableClick();
        pooHighlight.StopHighlight();
        textSequenceViewer.ContinueAfterExternalAction();
    }
}