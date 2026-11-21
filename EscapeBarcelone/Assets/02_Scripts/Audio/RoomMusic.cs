using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Un objet de la salle qui peut imposer la musique (ex. une radio allumée,
/// ou l'écho d'une radio allumée dans la salle d'à côté).
/// </summary>
public interface IRoomMusicSource
{
    /// <returns>true si cet objet veut jouer de la musique maintenant.</returns>
    bool TryGetMusic(out AudioData music, out float startPosition);
}

/// <summary>
/// Décide UNE fois, à un seul endroit, quelle musique jouer dans la salle courante :
/// 1. une source de la salle qui le demande (radio allumée, écho de radio) ;
/// 2. sinon la musique de la salle (RoomService) ;
/// 3. sinon le silence.
/// Appelé à l'arrivée dans une salle et quand une radio est allumée / éteinte.
/// Remplace l'ancien fonctionnement où la radio et la salle lançaient chacune leur
/// musique « une frame plus tard », dans un ordre qui n'était pas garanti.
/// </summary>
public static class RoomMusic
{
    public static void Refresh()
    {
        AudioManager audio = ServiceLocator.Get<AudioManager>();

        if (audio == null)
            return;

        Scene room = SceneManager.GetActiveScene();

        foreach (IRoomMusicSource source in FindSources(room))
        {
            if (source.TryGetMusic(out AudioData music, out float position))
            {
                audio.PlayMusic(music, position: position);
                return;
            }
        }

        RoomService service = ServiceLocator.Get<RoomService>();

        if (service != null && service.gameObject.scene == room)
            audio.PlayMusic(service.roomMusic);
        else
            audio.StopMusic();
    }

    private static IEnumerable<IRoomMusicSource> FindSources(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded)
            yield break;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (IRoomMusicSource source in root.GetComponentsInChildren<IRoomMusicSource>())
                yield return source;
        }
    }
}
