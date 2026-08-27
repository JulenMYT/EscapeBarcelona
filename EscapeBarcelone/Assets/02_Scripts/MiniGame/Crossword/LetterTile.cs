using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class LetterTile : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] protected TMP_InputField inputField;
    [SerializeField] protected Color selectedColor;
    [SerializeField] protected Color highlightedColor;
    [SerializeField] protected Color baseColor;
    [SerializeField] protected Color correctColor;

    public event Action<LetterTile> OnTileClicked;
    public event Action<LetterTile> OnLetterEntered;

    public bool IsLocked { get; private set; }

    private void Awake()
    {
        inputField.onValueChanged.AddListener(CheckLetter);
    }

    public void CheckLetter(string value)
    {
        if (string.IsNullOrEmpty(value))
            return;

        char enteredLetter = value[^1];

        if (!char.IsLetter(enteredLetter))
        {
            inputField.SetTextWithoutNotify(string.Empty);
            return;
        }

        inputField.SetTextWithoutNotify(char.ToUpper(enteredLetter).ToString());
        OnLetterEntered?.Invoke(this);
    }

    public void SetLetter(char letter)
    {
        inputField.SetTextWithoutNotify(char.ToUpper(letter).ToString());
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnTileClicked?.Invoke(this);
    }

    public void ClearLetter()
    {
        inputField.text = string.Empty;
    }

    public void Focus()
    {
        inputField.Select();
        inputField.ActivateInputField();
    }

    public bool HasLetter()
    {
        return !string.IsNullOrEmpty(inputField.text);
    }

    public char GetEnteredLetter()
    {
        return char.ToUpper(inputField.text[0]);
    }

    public void SetSelected()
    {
        if (!IsLocked)
            inputField.image.color = selectedColor;
    }

    public void SetHighlighted()
    {
        if (!IsLocked)
            inputField.image.color = highlightedColor;
    }

    public void SetBase()
    {
        if (!IsLocked)
            inputField.image.color = baseColor;
    }

    public void SetCorrect()
    {
        inputField.image.color = correctColor;
    }

    public void Lock()
    {
        IsLocked = true;
        inputField.interactable = false;
    }
}