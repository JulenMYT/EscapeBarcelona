using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class AnswerTile : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color wordHighlightColor;
    [SerializeField] private Color baseColor;
    [SerializeField] private Color correctColor;
    public bool IsLocked { get; private set; }

    public TileData TileData { get; private set; }

    private void Awake()
    {
        inputField.onValueChanged.AddListener(CheckLetter);
    }

    public void Setup(TileData tileData)
    {
        TileData = tileData;

        inputField.text = string.Empty;

        SetBase();
    }

    public void CheckLetter(string value)
    {
        if (string.IsNullOrEmpty(value))
            return;

        char enteredLetter = value[^1];

        if (!char.IsLetter(enteredLetter))
        {
            inputField.text = string.Empty;
            return;
        }

        inputField.text = char.ToUpper(enteredLetter).ToString();
    }

    public void ClearLetter()
    {
        inputField.text = string.Empty;
    }

    public void OnPointerClick(PointerEventData eventData)
    {

    }

    public void SetSelected()
    {
        if (!IsLocked)
            inputField.image.color = selectedColor;
    }

    public void SetHighlighted()
    {
        if (!IsLocked)
            inputField.image.color = wordHighlightColor;
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

    public void Lock()
    {
        IsLocked = true;
        inputField.interactable = false;
    }
}
