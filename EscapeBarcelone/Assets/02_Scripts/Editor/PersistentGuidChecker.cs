using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Tools > Escape Barcelone > Vérifier les identifiants persistants :
/// parcourt toutes les scènes du build, liste les PersistentGuid vides ou en double,
/// et propose de donner un nouvel identifiant aux doublons (le premier trouvé garde le sien).
/// </summary>
public static class PersistentGuidChecker
{
    private const string MenuPath = "Tools/Escape Barcelone/Vérifier les identifiants persistants";

    private class Entry
    {
        public string ScenePath;
        public string ObjectPath;
        public string Guid;
    }

    [MenuItem(MenuPath)]
    private static void Check()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        SceneSetup[] previousSetup = EditorSceneManager.GetSceneManagerSetup();
        List<Entry> entries = Collect();

        List<IGrouping<string, Entry>> duplicates = entries
            .Where(e => !string.IsNullOrEmpty(e.Guid))
            .GroupBy(e => e.Guid)
            .Where(g => g.Count() > 1)
            .ToList();

        List<Entry> empty = entries.Where(e => string.IsNullOrEmpty(e.Guid)).ToList();

        RestoreSetup(previousSetup);

        if (duplicates.Count == 0 && empty.Count == 0)
        {
            EditorUtility.DisplayDialog("Identifiants persistants",
                $"Tout est bon : {entries.Count} identifiants, tous uniques.", "OK");
            return;
        }

        string report = string.Join("\n\n", duplicates.Select(g =>
            $"Doublon {g.Key.Substring(0, 8)}… :\n" + string.Join("\n", g.Select(e => $"  • {e.ObjectPath}  ({SceneName(e.ScenePath)})"))));

        if (empty.Count > 0)
            report += "\n\nVides :\n" + string.Join("\n", empty.Select(e => $"  • {e.ObjectPath}  ({SceneName(e.ScenePath)})"));

        Debug.LogWarning("Identifiants persistants à corriger :\n" + report);

        bool fix = EditorUtility.DisplayDialog("Identifiants persistants",
            report + "\n\nDonner un nouvel identifiant aux doublons et aux vides ? " +
            "(Le premier objet de chaque doublon garde le sien.)",
            "Corriger", "Annuler");

        if (!fix)
            return;

        HashSet<(string, string)> toFix = new();

        foreach (IGrouping<string, Entry> group in duplicates)
        {
            foreach (Entry e in group.Skip(1))
                toFix.Add((e.ScenePath, e.ObjectPath));
        }

        foreach (Entry e in empty)
            toFix.Add((e.ScenePath, e.ObjectPath));

        Fix(toFix);
        RestoreSetup(previousSetup);

        Debug.Log($"Identifiants persistants : {toFix.Count} corrigé(s).");
    }

    private static List<Entry> Collect()
    {
        List<Entry> entries = new();

        foreach (string scenePath in BuildScenePaths())
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            foreach (PersistentGuid guid in FindIn(scene))
            {
                entries.Add(new Entry
                {
                    ScenePath = scenePath,
                    ObjectPath = GetPath(guid.transform),
                    Guid = guid.Guid
                });
            }
        }

        return entries;
    }

    private static IEnumerable<string> BuildScenePaths()
    {
        return EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path);
    }

    private static void Fix(HashSet<(string scene, string path)> toFix)
    {
        foreach (string scenePath in toFix.Select(f => f.scene).Distinct())
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            foreach (PersistentGuid guid in FindIn(scene))
            {
                if (toFix.Contains((scenePath, GetPath(guid.transform))))
                    guid.RegenerateGuid();
            }

            EditorSceneManager.SaveScene(scene);
        }
    }

    private static void RestoreSetup(SceneSetup[] setup)
    {
        if (setup != null && setup.Length > 0)
            EditorSceneManager.RestoreSceneManagerSetup(setup);
    }

    private static string SceneName(string path)
    {
        return System.IO.Path.GetFileNameWithoutExtension(path);
    }

    private static IEnumerable<PersistentGuid> FindIn(Scene scene)
    {
        return scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<PersistentGuid>(true));
    }

    private static string GetPath(Transform transform)
    {
        string path = transform.name;

        for (Transform parent = transform.parent; parent != null; parent = parent.parent)
            path = parent.name + "/" + path;

        return path;
    }
}
