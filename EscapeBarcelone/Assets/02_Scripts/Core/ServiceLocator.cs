using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Annuaire des systèmes du jeu (WorldState, InteractionManager…). Chaque système se
/// déclare dans son Awake avec Register, les autres le retrouvent avec Get.
///
/// - Get : le service est attendu. S'il manque (ou a été détruit), un avertissement est
///   affiché une fois dans l'éditeur, pour trouver tout de suite d'où vient le problème.
/// - TryGet : le service est optionnel (ex. une salle sans RoomService), aucun avertissement.
/// </summary>
public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> services = new();

#if UNITY_EDITOR
    private static readonly HashSet<Type> warned = new();
#endif

    private static bool isQuitting;

    // Remise à zéro à chaque Play (utile si le rechargement de domaine est désactivé).
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        services.Clear();
        isQuitting = false;
        Application.quitting += () => isQuitting = true;

#if UNITY_EDITOR
        warned.Clear();
#endif
    }

    public static void Register<T>(T service)
    {
        services[typeof(T)] = service;
    }

    public static T Get<T>()
    {
        if (TryGet(out T service))
            return service;

#if UNITY_EDITOR
        // À la fermeture du jeu, les systèmes disparaissent dans le désordre : c'est normal.
        if (!isQuitting && warned.Add(typeof(T)))
            Debug.LogWarning($"ServiceLocator : aucun {typeof(T).Name} disponible. " +
                             "Il n'est pas dans Bootstrap, pas encore chargé, ou a été détruit.");
#endif

        return default;
    }

    public static bool TryGet<T>(out T service)
    {
        if (services.TryGetValue(typeof(T), out object found))
        {
            // Objet Unity détruit (ex. RoomService d'une salle déchargée) : considéré comme absent.
            if (found is UnityEngine.Object unityObject && unityObject == null)
            {
                service = default;
                return false;
            }

            service = (T)found;
            return true;
        }

        service = default;
        return false;
    }
}
