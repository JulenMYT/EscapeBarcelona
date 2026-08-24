using System.Collections.Generic;
using UnityEngine;

public class CrosswordDefinition : MonoBehaviour
{
    [SerializeField] private RectTransform horizontalGroup;
    [SerializeField] private RectTransform verticalGroup;
    [SerializeField] private CrosswordDefinitionItem definitionPrefab;

    public void CreateDefinitions(CrosswordData data)
    {
        List<WordData> sortedWords = new(data.words);
        sortedWords.Sort((a, b) => a.Number.CompareTo(b.Number));

        foreach (WordData word in sortedWords)
        {
            RectTransform parent = word.isDown
                ? verticalGroup
                : horizontalGroup;

            CrosswordDefinitionItem definition = Instantiate(definitionPrefab, parent);
            definition.Setup(word);
        }
    }
}