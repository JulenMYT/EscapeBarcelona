using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomTransitionManager : MonoBehaviour
{
    /// <summary>Clé SessionState écrite par l'outil d'éditeur BootstrapPlayMode.</summary>
    public const string EditorStartRoomKey = "EscapeBarcelone.StartRoom";

    [SerializeField] private ScreenFader screenFader;
    [Tooltip("Salle chargée au lancement du jeu (build).")]
    [SerializeField] private string firstRoom = "Room1";

    private string currentRoom = "";
    private bool isTransitioning;

    /// <summary>Vrai pendant tout le changement de salle, fondus compris.</summary>
    public bool IsBusy { get; private set; }

    private void Awake()
    {
        ServiceLocator.Register<RoomTransitionManager>(this);
    }

    private void Start()
    {
        // Écran noir dès le départ : on ne voit pas la scène Bootstrap vide.
        screenFader.SetAlpha(1f);

        EnterRoom(GetStartRoom());
    }

    /// <summary>
    /// Salle à charger au démarrage :
    /// 1. une salle déjà chargée (ancien flux : Play lancé depuis une salle) ;
    /// 2. dans l'éditeur, la salle qui était ouverte quand on a appuyé sur Play ;
    /// 3. sinon firstRoom.
    /// </summary>
    private string GetStartRoom()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);

            if (scene != gameObject.scene && scene.isLoaded)
                return scene.name;
        }

#if UNITY_EDITOR
        string editorRoom = UnityEditor.SessionState.GetString(EditorStartRoomKey, string.Empty);
        UnityEditor.SessionState.EraseString(EditorStartRoomKey);

        if (!string.IsNullOrEmpty(editorRoom))
            return editorRoom;
#endif

        return firstRoom;
    }

    public void EnterRoom(string sceneName)
    {
        if (isTransitioning || string.IsNullOrEmpty(sceneName))
            return;

        StartCoroutine(Transition(sceneName));
    }

    private IEnumerator Transition(string sceneName)
    {
        isTransitioning = true;    //Lock
        IsBusy = true;

        // Aucun clic pendant le fondu : sinon on peut ouvrir un gros plan
        // dans une salle sur le point d'être déchargée.
        InteractionManager interactionManager = ServiceLocator.Get<InteractionManager>();
        interactionManager?.SetTransitionLock(true);

        if (screenFader.canvasGroup.alpha < 1f)
            yield return screenFader.Fade(screenFader.canvasGroup.alpha, 1f, 0.5f);

        // L'InspectionHandler vit dans Bootstrap : on vide sa pile avant de
        // décharger la salle pour ne pas garder de références vers des objets détruits.
        ServiceLocator.Get<InspectionHandler>()?.CloseAll();

        // On arrive toujours dans une nouvelle salle avec le carnet et la boussole fermés.
        ServiceLocator.Get<Book>()?.CloseBook();
        ServiceLocator.Get<Compass>()?.CloseCompass();

        // On ne décharge jamais la scène Bootstrap elle-même.
        if (!string.IsNullOrEmpty(currentRoom) && currentRoom != gameObject.scene.name)
            yield return SceneManager.UnloadSceneAsync(currentRoom);

        if (!SceneManager.GetSceneByName(sceneName).isLoaded)
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        Scene newScene = SceneManager.GetSceneByName(sceneName);

        if (newScene.IsValid())
            SceneManager.SetActiveScene(newScene);

        currentRoom = sceneName;

        yield return null;

        // Un seul endroit décide de la musique : radio (ou écho de radio) allumée,
        // sinon musique de la salle, sinon silence.
        RoomMusic.Refresh();

        isTransitioning = false;

        yield return screenFader.Fade(1f, 0f, 0.5f);

        interactionManager?.SetTransitionLock(false);
        IsBusy = false;
    }
}
