using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Un élément d'une page du carnet qui ouvre en gros plan une fiche du carnet lui-même
/// (ex. la fiche du guide sur Barcelone). Il faut un Button sur l'objet.
///
/// La fiche (« View ») est dans Bootstrap, à côté du carnet : un canvas désactivé, en
/// « Screen Space - Camera ». Le carnet se cache pendant le gros plan et revient à la même
/// page quand on ferme la fiche.
///
/// (Pour ouvrir quelque chose qui est dans une SALLE, comme les mots croisés, c'est BookLink.)
/// </summary>
[RequireComponent(typeof(Button))]
public class BookZoom : MonoBehaviour
{
    [Tooltip("La fiche à ouvrir en gros plan (désactivée dans la scène).")]
    [SerializeField] private GameObject view;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Open);
    }

    private void Open()
    {
        if (view == null)
        {
            Debug.LogWarning("BookZoom : aucune fiche (View) renseignée.", this);
            return;
        }

        ServiceLocator.Get<Book>()?.OpenFromBook(view);
    }
}
