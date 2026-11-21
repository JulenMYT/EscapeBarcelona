using UnityEngine;

/// <summary>
/// La radio qu'on allume / éteint en cliquant dessus. Sa musique peut aussi s'entendre
/// depuis d'autres salles grâce à des RadioEcho qui portent le même identifiant.
/// </summary>
public class Radio : Interaction, IRoomMusicSource
{
    [Tooltip("Nom partagé avec les RadioEcho des autres salles. Vide = identifiant du PersistentGuid.")]
    [SerializeField] private string radioId;

    [Tooltip("Son quand on est dans la même salle que la radio (mixage proche).")]
    [SerializeField] private AudioData sound;

    public string RadioId
    {
        get
        {
            if (!string.IsNullOrEmpty(radioId))
                return radioId;

            PersistentGuid guid = GetComponent<PersistentGuid>();
            return guid != null ? guid.Guid : name;
        }
    }

    protected override void Interact()
    {
        ToggleRadio();
    }

    public void ToggleRadio()
    {
        RadioState.SetOn(RadioId, !RadioState.IsOn(RadioId));

        // Radio allumée : sa musique. Éteinte : retour à la musique de la salle (ou silence).
        RoomMusic.Refresh();
    }

    public bool TryGetMusic(out AudioData music, out float startPosition)
    {
        music = sound;
        startPosition = RadioState.GetPosition(RadioId, sound.soundClip);
        return RadioState.IsOn(RadioId) && sound.soundClip != null;
    }
}
