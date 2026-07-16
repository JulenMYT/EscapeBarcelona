public class UIManager
{
    private static UIManager _instance;

    public static UIManager Instance => _instance ??= new UIManager();

    public FadePanel fadePanel { get; private set; }
    public FocusPanelGameplay focusPanelGameplay { get; private set; }
    public FocusPanelTutorial focusPanelTutorial { get; private set; }
    public GameplayPanel gameplayPanel { get; private set; }
    public DialogueUI dialogueUI { get; private set; }
    public TutorialUI tutorialUI { get; private set; }

    public void Initialize(
        FadePanel fadePanel,
        FocusPanelGameplay focusPanelGameplay,
        FocusPanelTutorial focusPanelTutorial,
        GameplayPanel gameplayPanel,
        DialogueUI dialogueUI,
        TutorialUI tutorialUI)
    {
        this.fadePanel = fadePanel;
        this.focusPanelGameplay = focusPanelGameplay;
        this.focusPanelTutorial = focusPanelTutorial;
        this.gameplayPanel = gameplayPanel;
        this.dialogueUI = dialogueUI;
        this.tutorialUI = tutorialUI;
    }
}