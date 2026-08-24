using TMPro;
using UnityEngine;

public class CrosswordDefinitionItem : MonoBehaviour
{
    [SerializeField] private TMP_Text definitionText;

    public void Setup(WordData word)
    {
        definitionText.text = $"{word.Number}. {word.hint}";
    }
}