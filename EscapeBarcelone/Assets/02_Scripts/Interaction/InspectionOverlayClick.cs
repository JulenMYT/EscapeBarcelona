using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Ajouté automatiquement sur l'écran sombre par InspectionHandler :
/// transmet les clics sur l'écran sombre (pour fermer en cliquant à côté du gros plan).
/// </summary>
public class InspectionOverlayClick : MonoBehaviour, IPointerClickHandler
{
    public event Action<PointerEventData> Clicked;

    public void OnPointerClick(PointerEventData eventData)
    {
        Clicked?.Invoke(eventData);
    }
}
