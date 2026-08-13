using UnityEngine;

public class PasswordMiniGame : MonoBehaviour
{
    [SerializeField] private Transform layoutGroup;
    [SerializeField] private LetterWheel prefab;
    private LetterWheel[] wheels;
    [SerializeField] private string password = "ZELDA";

    [SerializeField] private InteractionElementBase closedChest;
    [SerializeField] private InteractionElementBase openChest;

    [SerializeField] private CutsceneInitiator cutsceneInitiator;

    private GameplayPanel gameplayPanel;

    private bool passwordCompleted = false;


    private void Awake()
    {
        CreateWheels();
    }

    private void CreateWheels()
    {
        wheels = new LetterWheel[password.Length];

        for (int i = 0; i < password.Length; i++)
        {
            wheels[i] = Instantiate(prefab, layoutGroup);
        }
    }

    private void OnEnable()
    {

        RefreshVisualState();

        if (!passwordCompleted)
        {
            gameplayPanel.SetValidateButtonVisible(true);
            gameplayPanel.OnValidateButtonClicked += CheckInput;
        }
    }

    private void OnDisable()
    {
        if (gameplayPanel == null)
            return;

        gameplayPanel.SetValidateButtonVisible(false);
        gameplayPanel.OnValidateButtonClicked -= CheckInput;
    }

    public void CheckInput()
    {
        string result = "";

        foreach (LetterWheel wheel in wheels)
            result += wheel.Letter;

        if (result == password)
            CompletePassword();
    }

    private void CompletePassword()
    {
        passwordCompleted = true;

        StoryManager.SetFlag(StoryFlag.hasOpenedPasswordChest);

        gameplayPanel.SetValidateButtonVisible(false);
        gameplayPanel.OnValidateButtonClicked -= CheckInput;
        cutsceneInitiator.StartCutscene();
        RefreshVisualState();
    }

    private void RefreshVisualState()
    {
        if (passwordCompleted)
        {
            closedChest.Close();
            openChest.Open();
        }
        else
        {
            closedChest.Open();
            openChest.Close();
        }
    }
}