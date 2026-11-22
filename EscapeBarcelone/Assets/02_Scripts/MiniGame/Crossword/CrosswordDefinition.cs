using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// La liste des définitions (horizontales et verticales). Elle signale les clics sur une
/// définition et surligne celle du mot sélectionné dans la grille.
/// </summary>
public class CrosswordDefinition : MonoBehaviour
{
    /// <summary>Le joueur a cliqué sur la définition de ce mot.</summary>
    public event Action<WordData> WordClicked;

    [SerializeField] private RectTransform horizontalGroup;
    [SerializeField] private RectTransform verticalGroup;
    [SerializeField] private CrosswordDefinitionItem definitionPrefab;

    private readonly List<CrosswordDefinitionItem> items = new();
    private CrosswordDefinitionItem highlighted;

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
            definition.Clicked += item => WordClicked?.Invoke(item.Word);
            items.Add(definition);
        }
    }

    /// <summary>Surligne la définition de ce mot (null : aucune).</summary>
    public void Highlight(WordData word)
    {
        if (highlighted != null)
            highlighted.SetHighlighted(false);

        highlighted = word == null ? null : items.Find(item => item.Word == word);

        if (highlighted != null)
            highlighted.SetHighlighted(true);
    }

    /// <summary>Grise la définition de ce mot (mot trouvé).</summary>
    public void SetCompleted(WordData word, bool completed)
    {
        CrosswordDefinitionItem item = items.Find(i => i.Word == word);

        if (item != null)
            item.SetCompleted(completed);
    }
}
