using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// Un élément cliquable DANS une page du carnet : une photo, un mot, une grille…
/// (ou la page entière, si on met le composant sur la page). Il faut un Button sur l'objet.
///
/// L'élément ne connaît pas ce qu'il ouvre : il demande au carnet d'ouvrir « Link Id »,
/// et c'est la salle courante qui répond grâce à un BookLinkTarget portant le même
/// identifiant (ex. les mots croisés de la Room 4).
///
/// Si aucune salle ne répond (ou si la condition n'est pas remplie), l'élément est grisé,
/// ou joue « Unavailable Dialogue » si tu en mets un.
/// </summary>
[RequireComponent(typeof(Button))]
public class BookLink : MonoBehaviour
{
    [Tooltip("Identifiant partagé avec le BookLinkTarget de la salle (ex. Crossword).")]
    [FormerlySerializedAs("pageId")]
    [SerializeField] private string linkId = "Crossword";

    [Tooltip("Flags nécessaires pour pouvoir utiliser cet élément. Vide = toujours.")]
    [SerializeField] private StoryCondition condition = new();

    [Tooltip("Optionnel : dialogue joué si l'élément ne peut pas servir ici. " +
             "Vide = l'élément est simplement grisé.")]
    [SerializeField] private Dialogue unavailableDialogue;

    private Button button;
    private Book book;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void OnEnable()
    {
        // L'élément est dans une page : il s'active quand sa page est affichée.
        if (ServiceLocator.TryGet(out book))
            book.LinkTargetsChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (book != null)
            book.LinkTargetsChanged -= Refresh;
    }

    private bool IsAvailable =>
        book != null && book.HasLinkTarget(linkId) && (condition == null || condition.IsMet());

    private void Refresh()
    {
        // Avec un dialogue d'explication, l'élément reste cliquable pour pouvoir le jouer.
        button.interactable = IsAvailable || unavailableDialogue != null;
    }

    private void OnClick()
    {
        if (IsAvailable)
        {
            book.OpenLink(linkId);
            return;
        }

        if (unavailableDialogue != null)
            ServiceLocator.Get<DialogueHandler>()?.StartDialogue(unavailableDialogue);
    }
}
