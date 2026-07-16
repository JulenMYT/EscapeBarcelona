using UnityEngine;

public class UIInitializer : MonoBehaviour
{
    [SerializeField] private FadePanel fadePanel;
    [SerializeField] private FocusPanelGameplay focusPanelGameplay;
    [SerializeField] private FocusPanelTutorial focusPanelTutorial;
    [SerializeField] private GameplayPanel gameplayPanel;
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private TutorialUI tutorialUI;

    private void Awake()
    {
        UIManager.Instance.Initialize(
            fadePanel,
            focusPanelGameplay,
            focusPanelTutorial,
            gameplayPanel,
            dialogueUI,
            tutorialUI
        );
    }
}