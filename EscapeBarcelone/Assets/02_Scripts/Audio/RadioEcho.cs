using UnityEngine;

/// <summary>
/// À placer dans une AUTRE salle que la radio : si la radio est allumée, on entend sa
/// musique d'ici, avec un mixage différent (ex. groupe « Far » : étouffé, plus faible).
/// La musique reprend exactement là où elle en est, comme si elle avait continué.
/// Pas cliquable : on ne peut allumer ou éteindre la radio que dans sa salle.
/// </summary>
public class RadioEcho : MonoBehaviour, IRoomMusicSource
{
    [Tooltip("Même identifiant que la Radio écoutée (son champ Radio Id, ou son PersistentGuid si vide).")]
    [SerializeField] private string radioId;

    [Tooltip("Même clip que la radio, avec le mixage « de loin » (ex. groupe Far).")]
    [SerializeField] private AudioData sound;

    private void Awake()
    {
        // Compatibilité : avant, l'écho était une Radio avec le même PersistentGuid.
        if (string.IsNullOrEmpty(radioId) && TryGetComponent(out PersistentGuid guid))
            radioId = guid.Guid;
    }

    public bool TryGetMusic(out AudioData music, out float startPosition)
    {
        music = sound;
        startPosition = RadioState.GetPosition(radioId, sound.soundClip);
        return !string.IsNullOrEmpty(radioId) && RadioState.IsOn(radioId) && sound.soundClip != null;
    }
}
