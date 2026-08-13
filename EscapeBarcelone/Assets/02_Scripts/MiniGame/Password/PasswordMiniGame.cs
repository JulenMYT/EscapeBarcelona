using UnityEngine;

public class PasswordMiniGame : MiniGame
{
    [SerializeField] private Transform layoutGroup;
    [SerializeField] private LetterWheel prefab;
    [SerializeField] private string password = "ZELDA";

    private LetterWheel[] wheels;
    private bool isCompleted;

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
            wheels[i].OnLetterChanged += CheckCompletion;
        }
    }

    private void CheckCompletion()
    {
        if (isCompleted)
            return;

        for (int i = 0; i < wheels.Length; i++)
        {
            if (wheels[i].Letter != password[i])
                return;
        }

        CompleteGame();
    }

    private void OnDestroy()
    {
        if (wheels == null)
            return;

        foreach (LetterWheel wheel in wheels)
        {
            if (wheel != null)
                wheel.OnLetterChanged -= CheckCompletion;
        }
    }

    private void CompleteGame()
    {
        ApplyCompletedState();
        TriggerGameCompleted();
    }

    private void ApplyCompletedState()
    {
        isCompleted = true;

        foreach (LetterWheel wheel in wheels)
            wheel.SetInteractable(false);
    }

    protected override void SolveMiniGame()
    {
        for (int i = 0; i < wheels.Length; i++)
            wheels[i].SetLetter(password[i]);

        ApplyCompletedState();
    }
}