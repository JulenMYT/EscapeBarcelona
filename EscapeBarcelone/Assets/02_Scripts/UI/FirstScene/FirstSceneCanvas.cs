using UnityEngine;
using UnityEngine.SceneManagement;

public class FirstSceneCanvas : MonoBehaviour
{
    [SerializeField] private TextSequenceViewer textSequenceViewer;
    [SerializeField] private TextSequence textSequence;

    private void Start()
    {
        Show();
    }

    private void OnEnable()
    {
        textSequenceViewer.OnInputEntered += HandleInputEntered;
        textSequenceViewer.OnSequenceCompleted += HandleSequenceCompleted;
    }

    private void OnDisable()
    {
        textSequenceViewer.OnInputEntered -= HandleInputEntered;
        textSequenceViewer.OnSequenceCompleted -= HandleSequenceCompleted;
    }

    public void Show()
    {
        textSequenceViewer.Play(textSequence);
    }

    private void HandleInputEntered(string value)
    {
        GameManager.Instance.SetPlayerName(value);
    }

    private void HandleSequenceCompleted()
    {
    }
}
