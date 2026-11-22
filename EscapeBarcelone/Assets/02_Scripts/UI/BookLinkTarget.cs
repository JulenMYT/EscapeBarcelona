using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// À placer dans une SALLE, sur un objet actif : répond aux clics sur l'élément du carnet
/// (BookLink) qui porte le même identifiant, en ouvrant « View » en gros plan.
/// Exemple : dans la Room 4, Link Id = Crossword, View = l'objet Crossword.
///
/// Il s'inscrit auprès du carnet quand la salle se charge et se retire quand elle se
/// décharge : dans les autres salles, l'élément du carnet n'ouvre rien.
/// </summary>
public class BookLinkTarget : MonoBehaviour
{
    [Tooltip("Identifiant partagé avec le BookLink du carnet (ex. Crossword).")]
    [FormerlySerializedAs("pageId")]
    [SerializeField] private string linkId = "Crossword";

    [Tooltip("Gros plan à ouvrir (il peut être désactivé dans la scène).")]
    [SerializeField] private GameObject view;

    public string LinkId => linkId;

    private Book book;

    /// <summary>Le gros plan à ouvrir (null si rien n'est renseigné).</summary>
    public GameObject View
    {
        get
        {
            if (view == null)
                Debug.LogWarning($"BookLinkTarget « {linkId} » : aucun gros plan (View) renseigné.", this);

            return view;
        }
    }

    private void OnEnable()
    {
        if (ServiceLocator.TryGet(out book))
            book.RegisterLinkTarget(this);
        else
            Debug.LogWarning($"BookLinkTarget « {linkId} » : aucun carnet (Book) trouvé dans Bootstrap.", this);
    }

    private void OnDisable()
    {
        if (book != null)
            book.UnregisterLinkTarget(this);
    }
}
