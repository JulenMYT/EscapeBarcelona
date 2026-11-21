using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// Quand on appuie sur Play depuis une salle, l'éditeur démarre en fait sur la scène
/// Bootstrap, qui charge ensuite la salle ouverte. Les services (WorldState,
/// InteractionManager…) existent donc toujours avant les objets de la salle,
/// exactement comme dans le build.
///
/// Désactivable via le menu Tools > Escape Barcelone > Lancer Play via Bootstrap.
/// </summary>
[InitializeOnLoad]
public static class BootstrapPlayMode
{
    private const string BootstrapSceneName = "Bootstrap";
    private const string MainMenuSceneName = "MainMenu";

    private const string MenuPath = "Tools/Escape Barcelone/Lancer Play via Bootstrap";
    private const string EnabledPrefKey = "EscapeBarcelone.BootstrapPlayMode";

    private static bool Enabled
    {
        get => EditorPrefs.GetBool(EnabledPrefKey, true);
        set => EditorPrefs.SetBool(EnabledPrefKey, value);
    }

    static BootstrapPlayMode()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

        // playModeStartScene doit être réglé avant d'appuyer sur Play :
        // on le met à jour à chaque changement de scène dans l'éditeur.
        EditorSceneManager.activeSceneChangedInEditMode += (_, _) => UpdateStartScene();
        EditorSceneManager.sceneOpened += (_, _) => UpdateStartScene();
        EditorApplication.delayCall += UpdateStartScene;
    }

    [MenuItem(MenuPath)]
    private static void Toggle()
    {
        Enabled = !Enabled;
        UpdateStartScene();
    }

    [MenuItem(MenuPath, true)]
    private static bool ToggleValidate()
    {
        Menu.SetChecked(MenuPath, Enabled);
        return true;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            UpdateStartScene();
            return;
        }

        if (state != PlayModeStateChange.ExitingEditMode)
            return;

        SessionState.EraseString(RoomTransitionManager.EditorStartRoomKey);
        UpdateStartScene(force: true);

        if (EditorSceneManager.playModeStartScene == null)
            return;

        // La salle sera rechargée depuis le disque : il faut l'enregistrer d'abord.
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorApplication.isPlaying = false;
            return;
        }

        SessionState.SetString(RoomTransitionManager.EditorStartRoomKey, SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Démarre sur Bootstrap si la scène active est une salle du build,
    /// sinon laisse Unity démarrer normalement.
    /// </summary>
    private static void UpdateStartScene() => UpdateStartScene(force: false);

    private static void UpdateStartScene(bool force)
    {
        // Pendant ExitingEditMode, Unity indique déjà 'en train de passer en Play' : d'où force.
        if (!force && EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        EditorSceneManager.playModeStartScene = ShouldStartFromBootstrap(SceneManager.GetActiveScene())
            ? FindBootstrapScene()
            : null;
    }

    private static bool ShouldStartFromBootstrap(Scene activeScene)
    {
        if (!Enabled)
            return false;

        if (activeScene.name == BootstrapSceneName || activeScene.name == MainMenuSceneName)
            return false;

        // Uniquement pour les salles du build (pas pour les scènes de test).
        return IsInBuild(activeScene.path);
    }

    private static bool IsInBuild(string scenePath)
    {
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled && scene.path == scenePath)
                return true;
        }

        return false;
    }

    private static SceneAsset FindBootstrapScene()
    {
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (Path.GetFileNameWithoutExtension(scene.path) == BootstrapSceneName)
                return AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
        }

        return null;
    }
}
