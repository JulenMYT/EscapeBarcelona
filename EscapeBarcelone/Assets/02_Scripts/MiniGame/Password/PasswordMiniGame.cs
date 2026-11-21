using UnityEngine;

public class PasswordMiniGame : MiniGame
{
    [SerializeField] private Transform layoutGroup;
    [SerializeField] private LetterWheel prefab;
    [SerializeField] private string password = "ZELDA";

    private LetterWheel[] wheels;

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
        for (int i = 0; i < wheels.Length; i++)
        {
            if (wheels[i].Letter != char.ToUpperInvariant(password[i]))
                return;
        }

        CompleteGame();
    }

#if UNITY_EDITOR
    // Prévient dans l'inspecteur si le mot de passe contient un caractère absent des roues
    // (espace, accent, chiffre…) : le mini-jeu serait alors impossible à résoudre.
    private void OnValidate()
    {
        foreach (char c in password)
        {
            if (LetterWheel.Alphabet.IndexOf(char.ToUpperInvariant(c)) < 0)
            {
                UnityEngine.Debug.LogWarning($"PasswordMiniGame : le caractère « {c} » du mot de passe « {password} » n'existe pas sur les roues, le mini-jeu est impossible.", this);
                return;
            }
        }
    }
#endif

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