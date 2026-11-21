using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Chef d'orchestre des cutscenes : une seule à la fois, les suivantes attendent leur
/// tour dans une file. Pendant une cutscene, TOUTES les entrées du joueur sont bloquées :
/// - interactions du décor (InteractionManager.Lock) ;
/// - toute l'UI : carnet, boussole, HUD, boutons, mots croisés… (un écran invisible
///   bloque les clics par-dessus, sous le fondu noir) ;
/// - clavier (le champ de texte sélectionné est désélectionné) ;
/// - fermeture des gros plans (bouton, clic à côté, Échap, clic droit).
/// Seuls passent : l'avancée des dialogues / tutos, et les objets qu'un tuto autorise
/// explicitement (InteractionManager.AllowObject).
///
/// Placé dans Bootstrap (objet [Systems]/CutsceneDirector), comme les autres systèmes.
/// S'il manque, il est créé automatiquement au premier besoin (secours).
/// </summary>
public class CutsceneDirector : MonoBehaviour
{
    [Tooltip("Ordre d'affichage de l'écran invisible qui bloque les clics. Au-dessus du carnet / " +
             "boussole (90) et des dialogues / tutos (100), sous le fondu noir (1000).")]
    [SerializeField] private int blockerSortingOrder = 500;

    private static CutsceneDirector instance;
    private static bool isQuitting;

    /// <summary>Vrai pendant une cutscene (et jusqu'à la frame qui suit sa fin).</summary>
    public static bool IsPlaying => instance != null && instance.isBlocking;

    private static readonly HashSet<GameObject> uiPassthrough = new();

    /// <summary>Éléments d'UI cliquables même pendant une cutscene.</summary>
    public static IReadOnlyCollection<GameObject> UIPassthrough => uiPassthrough;

    /// <summary>
    /// Autorise (ou non) un élément d'UI à recevoir les clics pendant une cutscene.
    /// Ex. : le bouton fermer d'un gros plan, quand un tuto demande de fermer.
    /// </summary>
    public static void SetUIPassthrough(GameObject element, bool allowed)
    {
        if (element == null)
            return;

        if (allowed)
            uiPassthrough.Add(element);
        else
            uiPassthrough.Remove(element);
    }

    public static event Action CutsceneStarted;
    public static event Action CutsceneFinished;

    private readonly List<CutsceneHandler> queue = new();
    private CutsceneHandler current;
    private Coroutine endRoutine;
    private bool isBlocking;
    private GameObject blocker;

    public static CutsceneDirector Instance
    {
        get
        {
            if (instance == null && !isQuitting)
            {
                // Normalement, celui de Bootstrap s'est déjà déclaré dans Awake.
                instance = FindAnyObjectByType<CutsceneDirector>();

                if (instance == null)
                {
                    Debug.LogWarning("CutsceneDirector absent de Bootstrap : création automatique.");
                    GameObject go = new GameObject("[CutsceneDirector]");
                    DontDestroyOnLoad(go);
                    instance = go.AddComponent<CutsceneDirector>();
                }
            }

            return instance;
        }
    }

    // Remet l'état statique à zéro à chaque Play (utile si le rechargement de domaine est désactivé).
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
        isQuitting = false;
        CutsceneStarted = null;
        CutsceneFinished = null;
        uiPassthrough.Clear();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }

        instance = this;
        ServiceLocator.Register<CutsceneDirector>(this);

        CreateBlocker();
        blocker.SetActive(false);
    }

    private void OnApplicationQuit()
    {
        isQuitting = true;
    }

    // ───────────────────────── File de cutscenes ─────────────────────────

    public void Play(CutsceneHandler handler)
    {
        if (handler == null || handler == current || queue.Contains(handler))
            return;

        if (current != null)
        {
            queue.Add(handler);
            return;
        }

        // Une cutscene vient de finir (déblocage prévu à la frame suivante) :
        // on enchaîne directement sans débloquer entre les deux.
        if (endRoutine != null)
        {
            StopCoroutine(endRoutine);
            endRoutine = null;
        }

        Begin(handler);
    }

    /// <summary>Appelé par CutsceneHandler quand sa cutscene est finie (ou détruite).</summary>
    public void NotifyFinished(CutsceneHandler handler)
    {
        if (handler != current)
        {
            queue.Remove(handler);
            return;
        }

        current = null;
        endRoutine = StartCoroutine(EndNextFrame());
    }

    private void Begin(CutsceneHandler handler)
    {
        current = handler;

        bool wasBlocking = isBlocking;
        Block();

        if (!wasBlocking)
            CutsceneStarted?.Invoke();

        StartCoroutine(BeginWhenRoomReady(handler));
    }

    // Une cutscene lancée à l'arrivée dans une salle attend la fin du fondu :
    // sinon son premier dialogue commence sous l'écran noir.
    private IEnumerator BeginWhenRoomReady(CutsceneHandler handler)
    {
        RoomTransitionManager transition = ServiceLocator.Get<RoomTransitionManager>();

        while (transition != null && transition.IsBusy)
            yield return null;

        // La salle a pu être déchargée pendant l'attente : on libère la place, sinon
        // le jeu resterait bloqué en attendant une cutscene qui n'existe plus.
        if (handler == null)
        {
            current = null;
            endRoutine = StartCoroutine(EndNextFrame());
            yield break;
        }

        if (handler != current)
            yield break;

        handler.BeginPlayback();
    }

    // On attend une frame : le clic qui termine le dernier dialogue ne doit pas
    // déclencher en plus une interaction du décor.
    private IEnumerator EndNextFrame()
    {
        yield return null;

        endRoutine = null;

        while (queue.Count > 0)
        {
            CutsceneHandler next = queue[0];
            queue.RemoveAt(0);

            if (next != null && next.isActiveAndEnabled)
            {
                Begin(next);
                yield break;
            }
        }

        // Quelqu'un demande encore à bloquer (animation avant une cutscene…) : on garde le blocage.
        if (holds.Count > 0)
            yield break;

        Unblock();
        CutsceneFinished?.Invoke();
    }

    // ───────────────────────── Blocage hors cutscene ─────────────────────────

    private readonly HashSet<object> holds = new();

    /// <summary>
    /// Bloque les entrées comme pendant une cutscene, sans en jouer une. Utile pour un
    /// moment scripté juste avant une cutscene (ex. animation d'ouverture du coffre).
    /// Toujours appeler Release ensuite.
    /// </summary>
    public void Hold(object owner)
    {
        if (!holds.Add(owner))
            return;

        if (endRoutine != null)
        {
            StopCoroutine(endRoutine);
            endRoutine = null;
        }

        if (!isBlocking)
        {
            Block();
            CutsceneStarted?.Invoke();
        }
    }

    public void Release(object owner)
    {
        if (!holds.Remove(owner))
            return;

        // Une cutscene en cours (ou en attente de fin) gère elle-même le déblocage.
        if (holds.Count == 0 && current == null && endRoutine == null && isBlocking)
            endRoutine = StartCoroutine(EndNextFrame());
    }

    // ───────────────────────── Blocage des entrées ─────────────────────────

    private void Block()
    {
        isBlocking = true;

        ServiceLocator.Get<InteractionManager>()?.Lock();
        ServiceLocator.Get<InspectionHandler>()?.SetCanClose(false);

        // Plus de saisie clavier dans un champ de texte (mots croisés…).
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        blocker.SetActive(true);
    }

    private void Unblock()
    {
        isBlocking = false;

        ServiceLocator.Get<InteractionManager>()?.Unlock();
        ServiceLocator.Get<InspectionHandler>()?.SetCanClose(true);

        blocker.SetActive(false);
    }

    private void CreateBlocker()
    {
        blocker = new GameObject("InputBlocker", typeof(RectTransform));
        blocker.transform.SetParent(transform, false);

        Canvas canvas = blocker.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = blockerSortingOrder;

        blocker.AddComponent<GraphicRaycaster>();

        GameObject catcher = new GameObject("Catcher", typeof(RectTransform));
        catcher.transform.SetParent(blocker.transform, false);

        RectTransform rect = (RectTransform)catcher.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = catcher.AddComponent<Image>();
        image.color = Color.clear;   // invisible, mais bloque quand même les clics
        image.raycastTarget = true;

        catcher.AddComponent<CutsceneInputBlockerFilter>();
    }
}
