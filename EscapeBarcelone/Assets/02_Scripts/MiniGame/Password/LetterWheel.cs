using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LetterWheel : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private Button upButton;
    [SerializeField] private Button downButton;

    private const string alphabet = "ABCDEFGHIJKLMNÑOPQRSTUVWXYZ";

    private int index;

    public char Letter => alphabet[index];

    private void Awake()
    {
        upButton.onClick.AddListener(PreviousLetter);
        downButton.onClick.AddListener(NextLetter);
    }

    public void NextLetter()
    {
        index = (index + 1) % alphabet.Length;
        Refresh();
    }

    public void PreviousLetter()
    {
        index--;

        if (index < 0)
            index = alphabet.Length - 1;

        Refresh();
    }

    private void Refresh()
    {
        text.text = Letter.ToString();
    }
}