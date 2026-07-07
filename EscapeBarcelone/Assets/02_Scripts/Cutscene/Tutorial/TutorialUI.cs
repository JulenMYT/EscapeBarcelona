using TMPro;
using UnityEngine;

public class TutorialUI : Panel
{
    [SerializeField] private TMP_Text tutorialText;

    protected override void Initialize()
    {
        base.Initialize();
        Hide();
    }

    public void DisplayLine(string line)
    {
        tutorialText.text = line;
    }
}
