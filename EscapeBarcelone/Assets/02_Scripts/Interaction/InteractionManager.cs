using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class InteractionManager : MonoBehaviour
{
    /// <summary>Déclenché dès que ce qui est cliquable peut avoir changé.</summary>
    public static event Action StateChanged;

    [Header("Curseur (optionnel)")]
    [SerializeField] private Texture2D hoverCursor;
    [SerializeField] private Vector2 hoverCursorHotspot;

    private bool isLocked;
    private bool isTransitionLocked;
    private GameObject currentInspection;
    private Interaction hoveredInteraction;

    private readonly HashSet<GameObject> allowedObjects = new();
    private readonly HashSet<object> openPanels = new();

    private void Awake()
    {
        ServiceLocator.Register<InteractionManager>(this);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        EnsureRaycaster();
        NotifyStateChanged();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public bool CanInteract(GameObject obj)
    {
        if (isTransitionLocked)
            return false;

        if (isLocked)
            return allowedObjects.Contains(obj);

        // Carnet ou boussole ouvert : le décor derrière n'est pas cliquable.
        if (openPanels.Count > 0)
            return false;

        if (currentInspection != null)
            return obj.transform.IsChildOf(currentInspection.transform);

        return true;
    }

    public void Lock()
    {
        isLocked = true;
        allowedObjects.Clear();
        NotifyStateChanged();
    }

    public void Unlock()
    {
        isLocked = false;
        allowedObjects.Clear();
        NotifyStateChanged();
    }

    /// <summary>
    /// Verrou séparé de Lock/Unlock, pour qu'une cutscene lancée à l'arrivée
    /// dans une salle ne soit pas déverrouillée par la fin de la transition.
    /// </summary>
    public void SetTransitionLock(bool locked)
    {
        isTransitionLocked = locked;
        NotifyStateChanged();
    }

    /// <summary>
    /// Signale qu'un panneau du HUD (carnet, boussole…) est ouvert ou fermé.
    /// Tant qu'un panneau est ouvert, le décor n'est pas cliquable, sauf les objets
    /// autorisés explicitement par un tuto.
    /// </summary>
    public void SetPanelOpen(object panel, bool open)
    {
        bool changed = open ? openPanels.Add(panel) : openPanels.Remove(panel);

        if (changed)
            NotifyStateChanged();
    }

    /// <summary>
    /// Vrai si l'objet (ou un de ses parents) a été autorisé par un tuto pendant une cutscene.
    /// Utilisé par le bloqueur d'entrées pour laisser passer le clic.
    /// </summary>
    public bool IsAllowed(GameObject obj)
    {
        if (!isLocked || allowedObjects.Count == 0)
            return false;

        for (Transform t = obj.transform; t != null; t = t.parent)
        {
            if (allowedObjects.Contains(t.gameObject))
                return true;
        }

        return false;
    }

    public void AllowObject(GameObject obj)
    {
        allowedObjects.Add(obj);
        NotifyStateChanged();
    }

    public void DisallowObject(GameObject obj)
    {
        allowedObjects.Remove(obj);
        NotifyStateChanged();
    }

    public void SetCurrentInspection(GameObject inspection)
    {
        currentInspection = inspection;
        NotifyStateChanged();
    }

    public void ClearCurrentInspection()
    {
        currentInspection = null;
        NotifyStateChanged();
    }

    public void SetHovered(Interaction interaction, bool hovered)
    {
        if (hovered)
            hoveredInteraction = interaction;
        else if (hoveredInteraction == interaction)
            hoveredInteraction = null;
        else
            return;

        if (hoverCursor != null)
            Cursor.SetCursor(hoveredInteraction != null ? hoverCursor : null, hoverCursorHotspot, CursorMode.Auto);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureRaycaster();
        NotifyStateChanged();
    }

    /// <summary>
    /// Les Interaction reçoivent les clics via l'EventSystem : la caméra doit avoir
    /// un Physics2DRaycaster. On l'ajoute si besoin (tu peux aussi le mettre dans la scène).
    /// </summary>
    private static void EnsureRaycaster()
    {
        Camera cam = Camera.main;

        if (cam != null && cam.GetComponent<Physics2DRaycaster>() == null)
            cam.gameObject.AddComponent<Physics2DRaycaster>();
    }

    private static void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }
}
