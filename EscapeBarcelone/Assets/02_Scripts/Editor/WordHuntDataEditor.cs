using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Inspecteur de WordHuntData (la grille de la boussole) : plus besoin d'écrire les lignes à la main.
/// 1. Écris les destinations (et éventuellement des faux lieux dans « Decoy Words »).
/// 2. Choisis la taille de la grille et les directions autorisées.
/// 3. Clique sur « Générer la grille » : les mots sont placés au hasard et les cases vides
///    remplies de lettres. Re-clique pour une autre disposition.
/// L'aperçu montre où sont les mots, et les vérifications préviennent si un lieu est
/// introuvable ou présent deux fois.
/// </summary>
[CustomEditor(typeof(WordHuntData))]
public class WordHuntDataEditor : Editor
{
    private const int GenerationAttempts = 500;
    private const float CellSize = 20f;
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private static readonly Color BackgroundColor = new(0.18f, 0.18f, 0.18f);
    private static readonly Color CellColor = new(0.92f, 0.92f, 0.92f);
    private static readonly Color DestinationColor = new(1f, 0.85f, 0.4f);
    private static readonly Color DecoyColor = new(0.6f, 0.8f, 1f);

    private SerializedProperty destinations, decoyWords, gridSize, allowDiagonal, allowReversed, rows;

    private string lastMessage;
    private MessageType lastMessageType;
    private bool showRows;

    private void OnEnable()
    {
        destinations = serializedObject.FindProperty("destinations");
        decoyWords = serializedObject.FindProperty("decoyWords");
        gridSize = serializedObject.FindProperty("gridSize");
        allowDiagonal = serializedObject.FindProperty("allowDiagonal");
        allowReversed = serializedObject.FindProperty("allowReversed");
        rows = serializedObject.FindProperty("rows");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.HelpBox(
            "Écris les destinations (et des faux lieux si tu veux), puis clique sur « Générer la grille ». " +
            "Les lettres sont placées pour toi.",
            MessageType.Info);

        EditorGUILayout.PropertyField(destinations, true);
        EditorGUILayout.PropertyField(decoyWords, true);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Grille", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(gridSize, new GUIContent("Taille (colonnes, lignes)"));
        EditorGUILayout.PropertyField(allowDiagonal, new GUIContent("Diagonales"));
        EditorGUILayout.PropertyField(allowReversed, new GUIContent("À l'envers"));

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space(6);

        if (GUILayout.Button("Générer la grille", GUILayout.Height(28)))
            Generate();

        if (!string.IsNullOrEmpty(lastMessage))
            EditorGUILayout.HelpBox(lastMessage, lastMessageType);

        List<string> grid = CurrentRows();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Vérifications", EditorStyles.boldLabel);
        Dictionary<Vector2Int, Color> highlights = DrawValidation(grid);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Aperçu", EditorStyles.boldLabel);
        DrawPreview(grid, highlights);

        EditorGUILayout.Space(10);
        showRows = EditorGUILayout.Foldout(showRows, "Lignes (modification manuelle)", true);

        if (showRows)
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(rows, true);
            serializedObject.ApplyModifiedProperties();
        }
    }

    // ───────────────────────── Génération ─────────────────────────

    private void Generate()
    {
        serializedObject.Update();

        Vector2Int size = gridSize.vector2IntValue;
        List<string> words = DestinationWords().Concat(DecoyWords()).Where(w => w.Length > 1).Distinct().ToList();

        if (size.x < 2 || size.y < 2)
        {
            SetMessage("La grille doit faire au moins 2 × 2.", MessageType.Error);
            return;
        }

        if (words.Count == 0)
        {
            SetMessage("Ajoute d'abord au moins une destination.", MessageType.Warning);
            return;
        }

        string tooLong = words.FirstOrDefault(w => w.Length > Mathf.Max(size.x, size.y));

        if (tooLong != null)
        {
            SetMessage($"« {tooLong} » ({tooLong.Length} lettres) ne rentre pas dans une grille de {size.x} × {size.y}.", MessageType.Error);
            return;
        }

        System.Random random = new();
        List<Vector2Int> directions = Directions(allowDiagonal.boolValue, allowReversed.boolValue);
        List<string> destinationWords = DestinationWords().ToList();

        for (int attempt = 0; attempt < GenerationAttempts; attempt++)
        {
            char[,] grid = TryPlace(words, size, directions, random);

            if (grid == null)
                continue;

            FillBlanks(grid, random);
            List<string> result = ToRows(grid);

            // Les lettres au hasard ne doivent pas former un second exemplaire d'une destination.
            if (destinationWords.Any(w => FindAll(result, w).Count != 1))
                continue;

            rows.ClearArray();

            for (int i = 0; i < result.Count; i++)
            {
                rows.InsertArrayElementAtIndex(i);
                rows.GetArrayElementAtIndex(i).stringValue = result[i];
            }

            serializedObject.ApplyModifiedProperties();
            SetMessage($"Grille générée ({size.x} × {size.y}, {words.Count} mot(s)).", MessageType.Info);
            return;
        }

        SetMessage("Impossible de placer tous les mots : agrandis la grille, autorise les diagonales " +
                   "ou retire des faux lieux.", MessageType.Error);
    }

    private static char[,] TryPlace(List<string> words, Vector2Int size, List<Vector2Int> directions, System.Random random)
    {
        char[,] grid = new char[size.y, size.x];

        // Les plus longs d'abord : ce sont les plus difficiles à caser.
        foreach (string word in words.OrderByDescending(w => w.Length).ThenBy(_ => random.Next()))
        {
            List<(Vector2Int start, Vector2Int direction)> options = new();

            foreach (Vector2Int direction in directions)
            {
                for (int y = 0; y < size.y; y++)
                {
                    for (int x = 0; x < size.x; x++)
                    {
                        if (Fits(grid, word, new Vector2Int(x, y), direction, size))
                            options.Add((new Vector2Int(x, y), direction));
                    }
                }
            }

            if (options.Count == 0)
                return null;

            (Vector2Int start, Vector2Int dir) = options[random.Next(options.Count)];

            for (int i = 0; i < word.Length; i++)
            {
                Vector2Int p = start + dir * i;
                grid[p.y, p.x] = word[i];
            }
        }

        return grid;
    }

    private static bool Fits(char[,] grid, string word, Vector2Int start, Vector2Int direction, Vector2Int size)
    {
        for (int i = 0; i < word.Length; i++)
        {
            Vector2Int p = start + direction * i;

            if (p.x < 0 || p.y < 0 || p.x >= size.x || p.y >= size.y)
                return false;

            // Deux mots peuvent se croiser, seulement sur une même lettre.
            char existing = grid[p.y, p.x];

            if (existing != '\0' && existing != word[i])
                return false;
        }

        return true;
    }

    private static void FillBlanks(char[,] grid, System.Random random)
    {
        for (int y = 0; y < grid.GetLength(0); y++)
        {
            for (int x = 0; x < grid.GetLength(1); x++)
            {
                if (grid[y, x] == '\0')
                    grid[y, x] = Alphabet[random.Next(Alphabet.Length)];
            }
        }
    }

    private static List<string> ToRows(char[,] grid)
    {
        List<string> result = new();

        for (int y = 0; y < grid.GetLength(0); y++)
        {
            char[] row = new char[grid.GetLength(1)];

            for (int x = 0; x < row.Length; x++)
                row[x] = grid[y, x];

            result.Add(new string(row));
        }

        return result;
    }

    private static List<Vector2Int> Directions(bool diagonal, bool reversed)
    {
        // y vers le bas : la ligne 0 est en haut de la grille.
        List<Vector2Int> result = new() { new(1, 0), new(0, 1) };

        if (diagonal)
        {
            result.Add(new(1, 1));
            result.Add(new(1, -1));
        }

        if (reversed)
            result.AddRange(result.Select(d => -d).ToList());

        return result;
    }

    // ───────────────────────── Vérifications ─────────────────────────

    private Dictionary<Vector2Int, Color> DrawValidation(List<string> grid)
    {
        Dictionary<Vector2Int, Color> highlights = new();
        List<string> problems = new();

        if (grid.Count == 0)
        {
            EditorGUILayout.HelpBox("La grille est vide : clique sur « Générer la grille ».", MessageType.Warning);
            return highlights;
        }

        if (grid.Any(r => r.Length != grid[0].Length))
        {
            EditorGUILayout.HelpBox("Toutes les lignes doivent avoir la même longueur.", MessageType.Error);
            return highlights;
        }

        foreach (string word in DecoyWords())
        {
            foreach (List<Vector2Int> cells in FindAll(grid, word))
                cells.ForEach(c => highlights[c] = DecoyColor);
        }

        foreach (string word in DestinationWords())
        {
            List<List<Vector2Int>> found = FindAll(grid, word);

            if (found.Count == 0)
                problems.Add($"« {word} » est introuvable dans la grille.");
            else if (found.Count > 1)
                problems.Add($"« {word} » apparaît {found.Count} fois.");

            foreach (List<Vector2Int> cells in found)
                cells.ForEach(c => highlights[c] = DestinationColor);
        }

        for (int i = 0; i < destinations.arraySize; i++)
        {
            SerializedProperty destination = destinations.GetArrayElementAtIndex(i);
            string scene = destination.FindPropertyRelative("sceneName").stringValue;
            string word = destination.FindPropertyRelative("word").stringValue;

            if (!string.IsNullOrWhiteSpace(scene) && !EditorBuildSettings.scenes.Any(s => s.path.EndsWith("/" + scene + ".unity")))
                problems.Add($"La scène « {scene} » ({word}) n'est pas dans la liste des scènes du build (normal si elle n'existe pas encore).");
        }

        if (problems.Count == 0)
            EditorGUILayout.HelpBox("Tout est bon : chaque destination est présente une seule fois.", MessageType.Info);
        else
            EditorGUILayout.HelpBox(string.Join("\n", problems), MessageType.Warning);

        return highlights;
    }

    /// <summary>Toutes les positions où le mot se lit, dans les 8 directions (comme en jeu).</summary>
    private static List<List<Vector2Int>> FindAll(List<string> grid, string word)
    {
        List<List<Vector2Int>> found = new();
        int height = grid.Count, width = grid[0].Length;

        // Un mot qui se lit pareil dans les deux sens ne doit être compté qu'une fois.
        HashSet<string> seen = new();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0)
                            continue;

                        List<Vector2Int> cells = new();

                        for (int i = 0; i < word.Length; i++)
                        {
                            int px = x + dx * i, py = y + dy * i;

                            if (px < 0 || py < 0 || px >= width || py >= height || px >= grid[py].Length)
                                break;

                            if (CrosswordText.Normalize(grid[py][px]) != word[i])
                                break;

                            cells.Add(new Vector2Int(px, py));
                        }

                        if (cells.Count != word.Length)
                            continue;

                        string key = string.Join(";", cells.OrderBy(c => c.x).ThenBy(c => c.y));

                        if (seen.Add(key))
                            found.Add(cells);
                    }
                }
            }
        }

        return found;
    }

    // ───────────────────────── Aperçu ─────────────────────────

    private static void DrawPreview(List<string> grid, Dictionary<Vector2Int, Color> highlights)
    {
        if (grid.Count == 0)
            return;

        int width = grid.Max(r => r.Length), height = grid.Count;
        Rect area = GUILayoutUtility.GetRect(width * CellSize, height * CellSize, GUILayout.ExpandWidth(false));

        GUIStyle letterStyle = new(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };
        letterStyle.normal.textColor = Color.black;

        EditorGUI.DrawRect(area, BackgroundColor);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < grid[y].Length; x++)
            {
                Rect cell = new(area.x + x * CellSize + 1, area.y + y * CellSize + 1, CellSize - 2, CellSize - 2);
                EditorGUI.DrawRect(cell, highlights.TryGetValue(new Vector2Int(x, y), out Color color) ? color : CellColor);
                GUI.Label(cell, grid[y][x].ToString(), letterStyle);
            }
        }

        EditorGUILayout.LabelField("Jaune : destinations. Bleu : faux lieux.", EditorStyles.miniLabel);
    }

    // ───────────────────────── Utilitaires ─────────────────────────

    private IEnumerable<string> DestinationWords()
    {
        for (int i = 0; i < destinations.arraySize; i++)
            yield return Normalize(destinations.GetArrayElementAtIndex(i).FindPropertyRelative("word").stringValue);
    }

    private IEnumerable<string> DecoyWords()
    {
        for (int i = 0; i < decoyWords.arraySize; i++)
        {
            string word = Normalize(decoyWords.GetArrayElementAtIndex(i).stringValue);

            if (word.Length > 1)
                yield return word;
        }
    }

    private List<string> CurrentRows()
    {
        List<string> result = new();

        for (int i = 0; i < rows.arraySize; i++)
            result.Add(rows.GetArrayElementAtIndex(i).stringValue ?? string.Empty);

        return result.Where(r => r.Length > 0).ToList();
    }

    /// <summary>Majuscules, sans accents ni espaces (« Sagrada Família » → SAGRADAFAMILIA).</summary>
    private static string Normalize(string word)
    {
        return new string((word ?? string.Empty).Where(char.IsLetter).Select(CrosswordText.Normalize).ToArray());
    }

    private void SetMessage(string message, MessageType type)
    {
        lastMessage = message;
        lastMessageType = type;
    }
}
