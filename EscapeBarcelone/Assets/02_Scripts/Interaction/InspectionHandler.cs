using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Gère les gros plans (« inspections ») sous forme de pile : chaque nouveau gros plan
/// passe au-dessus du précédent, avec l'écran sombre entre les deux.
///
/// L'ordre d'affichage est automatique : à l'ouverture, tous les sprites et canvas du
/// gros plan sont décalés pour passer juste au-dessus de l'écran sombre de leur niveau,
/// puis remis à leurs valeurs d'origine à la fermeture. Un gros plan fonctionne donc
/// à n'importe quel niveau, quels que soient les ordres réglés dans la scène.
/// </summary>
public class InspectionHandler : MonoBehaviour
{
    [SerializeField] private GameObject darkBackground;
    [SerializeField] private GameObject closeButton;
    [SerializeField] private SpriteRenderer darkBackgroundRenderer;

    [Header("Animation")]
    [SerializeField] private float animationDuration = 0.15f;
    [Tooltip("Taille de départ du gros plan quand il apparaît (1 = pas de zoom).")]
    [SerializeField] private float popStartScale = 0.92f;

    private const int BaseSortingOrder = 0;
    private const int SortingStep = 10;

    private class InspectionEntry
    {
        public GameObject view;
        public Vector3 originalScale;
        public readonly Dictionary<Renderer, int> rendererOrders = new();
        public readonly Dictionary<Canvas, int> canvasOrders = new();
        public Coroutine popRoutine;
    }

    private readonly Stack<InspectionEntry> inspectionStack = new();

    private bool canClose = true;

    // Gros plans que le joueur ne peut pas fermer (ex. un mini-jeu tutoriel pas encore réussi),
    // avec l'objet qui le demande. Un gros plan ouvert PAR-DESSUS (une photo depuis la carte)
    // reste fermable : seul le gros plan bloqué lui-même ne l'est pas.
    private readonly Dictionary<object, GameObject> closeBlockers = new();

    /// <summary>Le joueur peut-il fermer le gros plan du dessus (croix, clic à côté, Échap, clic droit) ?</summary>
    public bool CanPlayerClose => canClose && !IsTopViewBlocked();

    private bool IsTopViewBlocked()
    {
        if (inspectionStack.Count == 0)
            return false;

        GameObject topView = inspectionStack.Peek().view;

        foreach (GameObject blockedObject in closeBlockers.Values)
        {
            if (blockedObject != null && topView != null && blockedObject.transform.IsChildOf(topView.transform))
                return true;
        }

        return false;
    }
    private float overlayAlpha;
    private Coroutine overlayRoutine;

    /// <summary>Déclenché à chaque gros plan fermé, avec le gros plan concerné.</summary>
    public event Action<GameObject> OnInspectionClosed;

    public bool IsInspecting => inspectionStack.Count > 0;

    private void Awake()
    {
        ServiceLocator.Register<InspectionHandler>(this);

        overlayAlpha = darkBackgroundRenderer.color.a;
        SetupOverlayClick();
    }

    private void Update()
    {
        // canClose fait foi : une cutscene le met à faux, et ne le remet à vrai
        // (CSE_InspectionCloseButton) que quand elle veut laisser le joueur fermer.
        if (!IsInspecting || !CanPlayerClose || IsHudPanelOpen())
            return;

        bool escape = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        bool rightClick = Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;

        if (escape || rightClick)
            Close();
    }

    // ───────────────────────── Ouverture / fermeture ─────────────────────────

    public void Inspect(GameObject detailedView)
    {
        bool isFirst = inspectionStack.Count == 0;

        InspectionEntry entry = new InspectionEntry
        {
            view = detailedView,
            originalScale = detailedView.transform.localScale
        };

        inspectionStack.Push(entry);

        int overlayOrder = BaseSortingOrder + inspectionStack.Count * SortingStep;
        darkBackgroundRenderer.sortingOrder = overlayOrder;
        AssignCanvasCamera(detailedView);
        ApplySorting(entry, overlayOrder + 1);

        if (isFirst)
        {
            FitOverlayToCamera();
            darkBackground.SetActive(true);
            closeButton.SetActive(CanPlayerClose);
            FadeOverlay(overlayAlpha, deactivateAtEnd: false);
        }

        detailedView.SetActive(true);
        entry.popRoutine = StartCoroutine(PopIn(entry));

        ServiceLocator.Get<InteractionManager>()
            .SetCurrentInspection(detailedView);

        // Le nouveau gros plan du dessus est peut-être bloqué (mini-jeu tutoriel).
        RefreshCloseButton();
    }

    public void Close()
    {
        if (inspectionStack.Count == 0)
            return;

        CloseTop();

        if (inspectionStack.Count == 0)
        {
            OnStackEmptied();
            return;
        }

        ServiceLocator.Get<InteractionManager>()
            .SetCurrentInspection(inspectionStack.Peek().view);

        RefreshCloseButton();

        darkBackgroundRenderer.sortingOrder =
            BaseSortingOrder + inspectionStack.Count * SortingStep;
    }

    public void CloseAll()
    {
        if (inspectionStack.Count == 0)
            return;

        while (inspectionStack.Count > 0)
            CloseTop();

        OnStackEmptied();
    }

    /// <summary>
    /// Note : ne remet plus canClose à vrai quand la pile se vide. C'est la fin de la
    /// cutscene (CutsceneHandler) qui s'en charge ; sinon le bouton fermer réapparaissait
    /// en pleine cutscene après un CSE_Close.
    /// </summary>
    public void SetCanClose(bool canClose)
    {
        this.canClose = canClose;
        RefreshCloseButton();
    }

    /// <summary>
    /// Interdit au joueur de fermer tant que ce bloqueur est actif, même si une cutscene
    /// autorise la fermeture. Les fermetures par le code (CSE_Close, changement de salle)
    /// restent possibles.
    /// </summary>
    /// <param name="blockedObject">Le gros plan concerné, ou un objet qu'il contient.</param>
    public void AddCloseBlocker(object blocker, GameObject blockedObject)
    {
        closeBlockers[blocker] = blockedObject;
        RefreshCloseButton();
    }

    public void RemoveCloseBlocker(object blocker)
    {
        if (closeBlockers.Remove(blocker))
            RefreshCloseButton();
    }

    private void RefreshCloseButton()
    {
        // Pendant une cutscene, le bouton fermer n'est cliquable que s'il est autorisé ici
        // (sinon l'écran invisible du CutsceneDirector bloque le clic).
        CutsceneDirector.SetUIPassthrough(closeButton, CanPlayerClose);

        if (inspectionStack.Count > 0)
            closeButton.SetActive(CanPlayerClose);
    }

    private void CloseTop()
    {
        InspectionEntry entry = inspectionStack.Pop();

        if (entry.popRoutine != null)
            StopCoroutine(entry.popRoutine);

        // Le gros plan peut avoir été détruit entre-temps (changement de salle…).
        if (entry.view != null)
        {
            entry.view.transform.localScale = entry.originalScale;
            entry.view.SetActive(false);
        }

        RestoreSorting(entry);

        OnInspectionClosed?.Invoke(entry.view);
    }

    private void OnStackEmptied()
    {
        closeButton.SetActive(false);
        FadeOverlay(0f, deactivateAtEnd: true);

        ServiceLocator.Get<InteractionManager>()
            .ClearCurrentInspection();
    }

    private static bool IsHudPanelOpen()
    {
        Book book = ServiceLocator.Get<Book>();
        Compass compass = ServiceLocator.Get<Compass>();

        return (book != null && book.IsOpen) || (compass != null && compass.IsOpen);
    }

    // ───────────────────────── Ordre d'affichage ─────────────────────────

    private static void ApplySorting(InspectionEntry entry, int targetBaseOrder)
    {
        Renderer[] renderers = entry.view.GetComponentsInChildren<Renderer>(true);
        Canvas[] canvases = entry.view.GetComponentsInChildren<Canvas>(true);

        int minOrder = int.MaxValue;

        foreach (Renderer r in renderers)
            minOrder = Mathf.Min(minOrder, r.sortingOrder);

        foreach (Canvas c in canvases)
        {
            if (OwnsSorting(c))
                minOrder = Mathf.Min(minOrder, c.sortingOrder);
        }

        if (minOrder == int.MaxValue)
            return;

        // Décalage pour que l'élément le plus bas du gros plan arrive juste au-dessus
        // de l'écran sombre. Les écarts entre éléments sont conservés.
        int delta = targetBaseOrder - minOrder;

        if (delta == 0)
            return;

        foreach (Renderer r in renderers)
        {
            entry.rendererOrders[r] = r.sortingOrder;
            r.sortingOrder += delta;
        }

        foreach (Canvas c in canvases)
        {
            if (!OwnsSorting(c))
                continue;

            entry.canvasOrders[c] = c.sortingOrder;
            c.sortingOrder += delta;
        }
    }

    private static void RestoreSorting(InspectionEntry entry)
    {
        foreach (KeyValuePair<Renderer, int> pair in entry.rendererOrders)
        {
            if (pair.Key != null)
                pair.Key.sortingOrder = pair.Value;
        }

        foreach (KeyValuePair<Canvas, int> pair in entry.canvasOrders)
        {
            if (pair.Key != null)
                pair.Key.sortingOrder = pair.Value;
        }

        entry.rendererOrders.Clear();
        entry.canvasOrders.Clear();
    }

    /// <summary>
    /// Un gros plan en UI réglé sur « Screen Space - Camera » ne peut pas référencer la
    /// caméra : elle est dans Bootstrap, lui dans une salle. Sans caméra, Unity l'affiche
    /// comme un canvas « Overlay », par-dessus tout, y compris la croix de fermeture.
    /// On lui donne donc la caméra principale à l'ouverture.
    /// </summary>
    private static void AssignCanvasCamera(GameObject view)
    {
        Camera cam = Camera.main;

        if (cam == null)
            return;

        foreach (Canvas canvas in view.GetComponentsInChildren<Canvas>(true))
        {
            if (canvas.isRootCanvas && canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null)
                canvas.worldCamera = cam;
        }
    }

    // Un canvas imbriqué sans « Override Sorting » hérite de l'ordre de son parent.
    private static bool OwnsSorting(Canvas canvas)
    {
        return canvas.isRootCanvas || canvas.overrideSorting;
    }

    // ───────────────────────── Écran sombre ─────────────────────────

    /// <summary>Ajuste l'écran sombre à la vue réelle de la caméra (16:9, 21:9, 4:3…).</summary>
    private void FitOverlayToCamera()
    {
        Camera cam = Camera.main;

        if (cam == null || !cam.orthographic || darkBackgroundRenderer.sprite == null)
            return;

        float height = cam.orthographicSize * 2f;
        float width = height * cam.aspect;

        Vector3 spriteSize = darkBackgroundRenderer.sprite.bounds.size;
        Transform overlay = darkBackground.transform;
        Vector3 parentScale = overlay.parent != null ? overlay.parent.lossyScale : Vector3.one;

        // Petite marge pour ne jamais voir de bord.
        const float margin = 1.05f;

        overlay.localScale = new Vector3(
            width * margin / (spriteSize.x * parentScale.x),
            height * margin / (spriteSize.y * parentScale.y),
            1f);

        // Centré sur la caméra. Tous les clics passent par l'EventSystem, qui choisit
        // l'objet selon l'ordre d'affichage : le gros plan (au-dessus de l'écran sombre)
        // reçoit donc ses clics avant l'écran sombre, quelle que soit la profondeur.
        Vector3 camPosition = cam.transform.position;
        overlay.position = new Vector3(camPosition.x, camPosition.y, overlay.position.z);
    }

    private void FadeOverlay(float targetAlpha, bool deactivateAtEnd)
    {
        if (overlayRoutine != null)
            StopCoroutine(overlayRoutine);

        overlayRoutine = StartCoroutine(FadeOverlayRoutine(targetAlpha, deactivateAtEnd));
    }

    private IEnumerator FadeOverlayRoutine(float targetAlpha, bool deactivateAtEnd)
    {
        Color color = darkBackgroundRenderer.color;
        float startAlpha = deactivateAtEnd ? color.a : 0f;
        float time = 0f;

        while (time < animationDuration)
        {
            time += Time.unscaledDeltaTime;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, time / animationDuration);
            darkBackgroundRenderer.color = color;
            yield return null;
        }

        color.a = targetAlpha;
        darkBackgroundRenderer.color = color;

        if (deactivateAtEnd)
        {
            darkBackground.SetActive(false);
            darkBackgroundRenderer.sortingOrder = BaseSortingOrder;

            // Prêt pour la prochaine ouverture.
            color.a = overlayAlpha;
            darkBackgroundRenderer.color = color;
        }

        overlayRoutine = null;
    }

    private IEnumerator PopIn(InspectionEntry entry)
    {
        Transform view = entry.view.transform;
        Vector3 from = entry.originalScale * popStartScale;
        float time = 0f;

        while (time < animationDuration)
        {
            time += Time.unscaledDeltaTime;

            // Ease-out : démarre vite, ralentit à la fin.
            float t = 1f - Mathf.Pow(1f - Mathf.Clamp01(time / animationDuration), 3f);
            view.localScale = Vector3.LerpUnclamped(from, entry.originalScale, t);
            yield return null;
        }

        view.localScale = entry.originalScale;
        entry.popRoutine = null;
    }

    // ───────────────────────── Clic à côté du gros plan ─────────────────────────

    private void SetupOverlayClick()
    {
        if (darkBackground.GetComponent<Collider2D>() == null)
            darkBackground.AddComponent<BoxCollider2D>();

        InspectionOverlayClick click = darkBackground.GetComponent<InspectionOverlayClick>();

        if (click == null)
            click = darkBackground.AddComponent<InspectionOverlayClick>();

        click.Clicked += OnOverlayClicked;
    }

    private void OnOverlayClicked(PointerEventData eventData)
    {
        if (!IsInspecting || !CanPlayerClose)
            return;

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        // Un clic sur une partie du gros plan sans collider (son fond, par exemple)
        // arrive aussi sur l'écran sombre : on ne ferme que si on a cliqué à côté.
        Vector3 worldPoint = eventData.pointerCurrentRaycast.worldPosition;

        if (IsInsideView(inspectionStack.Peek().view, worldPoint))
            return;

        Close();
    }

    private static bool IsInsideView(GameObject view, Vector3 worldPoint)
    {
        if (view == null)
            return false;

        foreach (Renderer r in view.GetComponentsInChildren<Renderer>())
        {
            if (!r.enabled)
                continue;

            Bounds bounds = r.bounds;
            Vector3 point = new Vector3(worldPoint.x, worldPoint.y, bounds.center.z);

            if (bounds.Contains(point))
                return true;
        }

        // Éléments d'UI visibles (images, textes) des canvas du gros plan. On ignore les
        // RectTransform vides, dont les canvas eux-mêmes, souvent plus grands que l'écran.
        Vector3[] corners = new Vector3[4];

        foreach (Graphic graphic in view.GetComponentsInChildren<Graphic>())
        {
            if (!graphic.enabled)
                continue;

            graphic.rectTransform.GetWorldCorners(corners);

            float minX = Mathf.Min(corners[0].x, corners[2].x);
            float maxX = Mathf.Max(corners[0].x, corners[2].x);
            float minY = Mathf.Min(corners[0].y, corners[2].y);
            float maxY = Mathf.Max(corners[0].y, corners[2].y);

            if (worldPoint.x >= minX && worldPoint.x <= maxX && worldPoint.y >= minY && worldPoint.y <= maxY)
                return true;
        }

        return false;
    }
}
