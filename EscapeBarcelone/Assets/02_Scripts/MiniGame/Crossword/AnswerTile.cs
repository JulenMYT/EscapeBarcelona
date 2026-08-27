using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class AnswerTile : LetterTile
{
    [SerializeField] private TMP_Text numberText;

    public void Setup(int number)
    {
        numberText.text = number.ToString();
    }
}
