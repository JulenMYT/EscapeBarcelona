using TMPro;
using UnityEngine;

public class TutorialUI : Panel
{
    [SerializeField] private TMP_Text tutorialText;

    [SerializeField] private Typewriter typewriter = new();

    public bool IsTyping => typewriter.IsTyping;

    public void DisplayLine(string line)
    {
        typewriter.Play(this, tutorialText, line);
    }

    public void CompleteLine()
    {
        typewriter.Complete();
    }

    public void SetTextPosition(float position)
    {
        tutorialText.rectTransform.anchoredPosition = new Vector2(
            tutorialText.rectTransform.anchoredPosition.x,
            position
        );
    }

    public override void Show()
    {
        canvasGroup.alpha = 1.0f;
    }

    public override void Hide()
    {
        typewriter.Stop();
        base.Hide();
    }
}
