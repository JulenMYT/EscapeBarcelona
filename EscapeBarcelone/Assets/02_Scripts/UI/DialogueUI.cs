using TMPro;
using UnityEngine;

public class DialogueUI : Panel
{
    [SerializeField] private TMP_Text speakerName;
    [SerializeField] private TMP_Text dialogueText;

    [Tooltip("Optionnel : affiché seulement quand la réplique est entièrement écrite (ex. ContinuarBox).")]
    [SerializeField] private GameObject continueIndicator;

    [SerializeField] private Typewriter typewriter = new();

    public bool IsTyping => typewriter.IsTyping;

    private void Awake()
    {
        typewriter.Completed += OnLineCompleted;
    }

    public void DisplayLine(string speakerName, string lineText)
    {
        this.speakerName.text = speakerName;

        SetContinueIndicator(false);
        typewriter.Play(this, dialogueText, lineText);
    }

    public void CompleteLine()
    {
        typewriter.Complete();
    }

    public override void Show()
    {
        canvasGroup.alpha = 1.0f;
    }

    public override void Hide()
    {
        typewriter.Stop();
        SetContinueIndicator(false);
        base.Hide();
    }

    private void OnLineCompleted()
    {
        SetContinueIndicator(true);
    }

    private void SetContinueIndicator(bool visible)
    {
        if (continueIndicator != null)
            continueIndicator.SetActive(visible);
    }
}
