using UnityEngine;

/// <summary>
/// Pendant une cutscene, laisse passer les clics uniquement sur :
/// - les objets du décor qu'un tuto a autorisés (InteractionManager.AllowObject) ;
/// - les éléments d'UI autorisés (CutsceneDirector.SetUIPassthrough), par exemple
///   le bouton fermer d'un gros plan quand la cutscene l'a réactivé.
/// Tout le reste est bloqué.
/// </summary>
public class CutsceneInputBlockerFilter : MonoBehaviour, ICanvasRaycastFilter
{
    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        if (IsOverAllowedUI(screenPoint))
            return false;

        InteractionManager interactionManager = ServiceLocator.Get<InteractionManager>();
        Camera cam = Camera.main;

        if (interactionManager == null || cam == null)
            return true;

        Vector3 worldPoint = cam.ScreenToWorldPoint(screenPoint);

        foreach (Collider2D hit in Physics2D.OverlapPointAll(worldPoint))
        {
            // Objet autorisé sous le pointeur : le bloqueur se laisse traverser.
            if (interactionManager.IsAllowed(hit.gameObject))
                return false;
        }

        return true;
    }

    private static bool IsOverAllowedUI(Vector2 screenPoint)
    {
        foreach (GameObject element in CutsceneDirector.UIPassthrough)
        {
            if (element == null || !element.activeInHierarchy)
                continue;

            if (element.transform is not RectTransform rect)
                continue;

            Canvas canvas = element.GetComponentInParent<Canvas>();
            Camera canvasCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;

            if (RectTransformUtility.RectangleContainsScreenPoint(rect, screenPoint, canvasCamera))
                return true;
        }

        return false;
    }
}
