using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LetterWheel : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject previousButton;

    private const string alphabet = "ABCDEFGHIJKLMNÑOPQRSTUVWXYZ";

    private int index;

    public event Action OnLetterChanged;

    public char Letter => alphabet[index];

    public void NextLetter()
    {
        index = (index + 1) % alphabet.Length;
        Refresh();
        OnLetterChanged?.Invoke();
    }

    public void PreviousLetter()
    {
        index--;

        if (index < 0)
            index = alphabet.Length - 1;

        Refresh();
        OnLetterChanged?.Invoke();
    }

    private void Refresh()
    {
        text.text = Letter.ToString();
    }

    public void SetLetter(char value)
    {
        int newIndex = alphabet.IndexOf(char.ToUpperInvariant(value));

        if (newIndex < 0)
            return;

        index = newIndex;
        Refresh();
    }

    public void SetInteractable(bool value)
    {
        nextButton.SetActive(value);
        previousButton.SetActive(value);
    }
}