using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Une définition de la liste. Cliquer dessus sélectionne son mot dans la grille, et elle
/// est surlignée quand son mot est sélectionné dans la grille.
/// </summary>
public class CrosswordDefinitionItem : MonoBehaviour, IPointerClickHandler
{
    public event Action<CrosswordDefinitionItem> Clicked;

    [SerializeField] private TMP_Text definitionText;

    [Tooltip("Couleur du texte quand le mot de cette définition est sélectionné dans la grille.")]
    [SerializeField] private Color highlightedColor = new(0.85f, 0.35f, 0.1f, 1f);

    [Tooltip("Couleur du texte quand le mot de cette définition est trouvé.")]
    [SerializeField] private Color completedColor = new(0.55f, 0.55f, 0.55f, 0.7f);

    public WordData Word { get; private set; }

    private Color baseColor;
    private FontStyles baseStyle;

    private bool isHighlighted;
    private bool isCompleted;

    public void Setup(WordData word)
    {
        Word = word;
        baseColor = definitionText.color;
        baseStyle = definitionText.fontStyle;

        // Première lettre en majuscule, quelle que soit l'écriture dans les données.
        string hint = (word.hint ?? string.Empty).Trim();

        if (hint.Length > 0)
            hint = char.ToUpper(hint[0]) + hint.Substring(1);

        definitionText.text = $"{word.Number}. {hint}";

        // Le texte doit recevoir les clics.
        definitionText.raycastTarget = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (CutsceneDirector.IsPlaying || isCompleted)
            return;

        Clicked?.Invoke(this);
    }

    public void SetHighlighted(bool highlighted)
    {
        isHighlighted = highlighted;
        Refresh();
    }

    /// <summary>Mot trouvé : la définition est grisée et ne réagit plus aux clics.</summary>
    public void SetCompleted(bool completed)
    {
        isCompleted = completed;
        Refresh();
    }

    private void Refresh()
    {
        if (isCompleted)
        {
            definitionText.color = completedColor;
            definitionText.fontStyle = baseStyle;
            return;
        }

        definitionText.color = isHighlighted ? highlightedColor : baseColor;
        definitionText.fontStyle = isHighlighted ? baseStyle | FontStyles.Underline : baseStyle;
    }
}
