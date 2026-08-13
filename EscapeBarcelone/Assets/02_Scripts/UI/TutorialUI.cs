using TMPro;
using UnityEngine;

public class TutorialUI : Panel
{
    [SerializeField] private TMP_Text tutorialText;

    public void DisplayLine(string line)
    {
        tutorialText.text = line;
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
}