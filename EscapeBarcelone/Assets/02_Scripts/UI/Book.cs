using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Le carnet du joueur (dans Bootstrap, commun à toutes les salles).
///
/// PAGES : chaque page est un objet sous le carnet, listé dans « Pages ». Une page peut
/// avoir une condition de flags : elle n'apparaît qu'une fois débloquée (notes ajoutées
/// au fil de l'histoire). On tourne les pages avec les boutons Précédent / Suivant, une ou
/// deux pages à la fois (« Pages Per Spread »). Le carnet se rouvre là où on l'a laissé.
///
/// ÉLÉMENTS CLIQUABLES : un objet d'une page avec un BookLink ouvre quelque chose dans la
/// salle courante (voir BookLink / BookLinkTarget) ; avec un BookZoom, il ouvre en gros plan
/// une fiche du carnet lui-même. Dans les deux cas, le carnet se cache pendant le gros plan
/// et se rouvre (à la même page) quand on le ferme.
/// </summary>
public class Book : MonoBehaviour
{
    [Serializable]
    public class PageEntry
    {
        public GameObject page;

        [Tooltip("Flags nécessaires pour que cette page apparaisse. Vide = toujours.")]
        public StoryCondition condition = new();
    }

    [SerializeField] private GameObject bookUI;
    [SerializeField] private GameObject openButton;
    [SerializeField] private GameObject closeButton;

    [Header("Pages")]
    [SerializeField] private List<PageEntry> pages = new();

    [Tooltip("1 = une page à la fois, 2 = une double page (gauche + droite).")]
    [Range(1, 2)]
    [SerializeField] private int pagesPerSpread = 1;

    [SerializeField] private GameObject previousButton;
    [SerializeField] private GameObject nextButton;

    [Tooltip("Optionnel : texte « 1 / 3 ».")]
    [SerializeField] private TMP_Text pageNumberText;

    private bool isOpen = false;

    // Index (dans la liste des pages débloquées) de la première page affichée.
    private int firstVisiblePage;

    public bool IsOpen => isOpen;

    public event Action OnBookOpened;
    public event Action OnBookClosed;

    /// <summary>Déclenché quand on tourne les pages (ou qu'une page est débloquée).</summary>
    public event Action OnPageChanged;

    private void Awake()
    {
        ServiceLocator.Register<Book>(this);

        // Le carnet démarre toujours fermé, même s'il est resté actif dans la scène.
        // Le bouton d'ouverture n'est pas touché : c'est le HUD qui le débloque.
        bookUI.SetActive(false);
        closeButton.SetActive(false);
    }

    private void Start()
    {
        // Une page débloquée pendant que le carnet est ouvert apparaît tout de suite.
        if (ServiceLocator.TryGet(out WorldState worldState))
            worldState.OnStoryFlagAdded += _ => { if (isOpen) ShowPages(); };

        if (ServiceLocator.TryGet(out InspectionHandler inspection))
            inspection.OnInspectionClosed += OnInspectionClosed;
    }

    // ───────────────────────── Gros plans ouverts depuis le carnet ─────────────────────────
    // Le carnet s'affiche par-dessus tout (canvas Overlay) : on le cache pendant le gros plan,
    // puis on le rouvre à la même page quand le joueur ferme ce gros plan.

    private GameObject viewOpenedFromBook;

    /// <summary>Ouvre ce gros plan depuis le carnet ; le carnet revient quand on le ferme.</summary>
    public void OpenFromBook(GameObject view)
    {
        InspectionHandler inspection = ServiceLocator.Get<InspectionHandler>();

        if (view == null || inspection == null)
            return;

        CloseBook();
        viewOpenedFromBook = view;
        inspection.Inspect(view);
    }

    private void OnInspectionClosed(GameObject closedView)
    {
        if (viewOpenedFromBook == null || closedView != viewOpenedFromBook)
            return;

        viewOpenedFromBook = null;

        // Fermé par le jeu (changement de salle, cutscene de fin du mini-jeu…) : on ne
        // rouvre pas le carnet au milieu.
        bool transitioning = ServiceLocator.TryGet(out RoomTransitionManager rooms) && rooms.IsBusy;

        if (!transitioning && !CutsceneDirector.IsPlaying)
            OpenBook();
    }

    public void OpenBook()
    {
        if (isOpen) return;

        // Un seul panneau à la fois.
        ServiceLocator.Get<Compass>()?.CloseCompass();

        isOpen = true;

        bookUI.SetActive(true);
        openButton.SetActive(false);
        closeButton.SetActive(true);
        ShowPages();
        ServiceLocator.Get<InteractionManager>()?.SetPanelOpen(this, true);
        OnBookOpened?.Invoke();
    }

    public void CloseBook()
    {
        if (!isOpen) return;

        isOpen = false;

        bookUI.SetActive(false);
        openButton.SetActive(true);
        closeButton.SetActive(false);
        ServiceLocator.Get<InteractionManager>()?.SetPanelOpen(this, false);
        OnBookClosed?.Invoke();
    }

    // ───────────────────────── Pages ─────────────────────────

    /// <summary>Bouton « Suivant ».</summary>
    public void NextPage()
    {
        firstVisiblePage += pagesPerSpread;
        ShowPages();
    }

    /// <summary>Bouton « Précédent ».</summary>
    public void PreviousPage()
    {
        firstVisiblePage -= pagesPerSpread;
        ShowPages();
    }

    /// <summary>Ouvre le carnet directement sur cette page (si elle est débloquée).</summary>
    public void OpenAtPage(GameObject page)
    {
        int index = UnlockedPages().IndexOf(page);

        if (index >= 0)
            firstVisiblePage = index - index % pagesPerSpread;

        if (isOpen)
            ShowPages();
        else
            OpenBook();
    }

    private List<GameObject> UnlockedPages()
    {
        List<GameObject> unlocked = new();

        foreach (PageEntry entry in pages)
        {
            if (entry.page != null && (entry.condition == null || entry.condition.IsMet()))
                unlocked.Add(entry.page);
        }

        return unlocked;
    }

    private void ShowPages()
    {
        List<GameObject> unlocked = UnlockedPages();

        // Toujours sur le début d'une double page, et jamais hors des pages débloquées.
        int lastStart = Mathf.Max(0, (unlocked.Count - 1) / pagesPerSpread * pagesPerSpread);
        firstVisiblePage = Mathf.Clamp(firstVisiblePage - firstVisiblePage % pagesPerSpread, 0, lastStart);

        foreach (PageEntry entry in pages)
        {
            if (entry.page != null)
                entry.page.SetActive(false);
        }

        for (int i = firstVisiblePage; i < firstVisiblePage + pagesPerSpread && i < unlocked.Count; i++)
            unlocked[i].SetActive(true);

        if (previousButton != null)
            previousButton.SetActive(firstVisiblePage > 0);

        if (nextButton != null)
            nextButton.SetActive(firstVisiblePage + pagesPerSpread < unlocked.Count);

        if (pageNumberText != null)
        {
            int spreads = Mathf.Max(1, Mathf.CeilToInt(unlocked.Count / (float)pagesPerSpread));
            pageNumberText.text = $"{firstVisiblePage / pagesPerSpread + 1} / {spreads}";
        }

        OnPageChanged?.Invoke();
    }

    // ───────────────────────── Éléments cliquables ─────────────────────────
    // Le carnet est dans Bootstrap, ce que ses éléments ouvrent est dans les salles : ils ne
    // peuvent pas se référencer directement. Un BookLink demande donc par identifiant, et la
    // salle courante inscrit un BookLinkTarget qui répond à cet identifiant.

    private readonly Dictionary<string, BookLinkTarget> linkTargets = new();

    /// <summary>Déclenché quand une salle inscrit ou retire une cible.</summary>
    public event Action LinkTargetsChanged;

    public void RegisterLinkTarget(BookLinkTarget target)
    {
        linkTargets[target.LinkId] = target;
        LinkTargetsChanged?.Invoke();
    }

    public void UnregisterLinkTarget(BookLinkTarget target)
    {
        if (linkTargets.TryGetValue(target.LinkId, out BookLinkTarget current) && current == target)
        {
            linkTargets.Remove(target.LinkId);
            LinkTargetsChanged?.Invoke();
        }
    }

    /// <summary>Une salle chargée sait-elle répondre à cet identifiant ?</summary>
    public bool HasLinkTarget(string linkId)
    {
        return linkTargets.TryGetValue(linkId, out BookLinkTarget target) && target != null;
    }

    /// <summary>Ouvre ce que l'élément désigne dans la salle courante. Renvoie false si rien ne répond.</summary>
    public bool OpenLink(string linkId)
    {
        if (!HasLinkTarget(linkId))
            return false;

        OpenFromBook(linkTargets[linkId].View);
        return true;
    }
}
