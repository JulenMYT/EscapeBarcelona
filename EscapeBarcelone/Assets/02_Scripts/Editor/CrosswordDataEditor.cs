using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Inspecteur de CrosswordData : plus besoin de calculer les positions à la main.
/// 1. Écris les mots et leurs indices dans la liste « Words » (positions ignorées).
/// 2. Écris le mot final.
/// 3. Clique sur « Générer la grille » : positions, directions et cases du mot final
///    sont calculées automatiquement. Re-clique pour obtenir une autre disposition.
/// L'aperçu et les vérifications en bas montrent la grille telle qu'elle sera en jeu.
/// </summary>
[CustomEditor(typeof(CrosswordData))]
public class CrosswordDataEditor : Editor
{
    private const int GenerationAttempts = 300;
    private const float CellSize = 22f;

    private static readonly Color EmptyColor = new(0.18f, 0.18f, 0.18f);
    private static readonly Color CellColor = new(0.92f, 0.92f, 0.92f);
    private static readonly Color FinalColor = new(1f, 0.85f, 0.4f);
    private static readonly Color ConflictColor = new(1f, 0.45f, 0.45f);

    private string lastMessage;
    private MessageType lastMessageType;

    public override void OnInspectorGUI()
    {
        CrosswordData data = (CrosswordData)target;

        EditorGUILayout.HelpBox(
            "Écris seulement les mots, leurs indices et le mot final, puis clique sur « Générer la grille ». " +
            "Les positions (Origin Position, Is Down) et les cases du mot final sont calculées pour toi.",
            MessageType.Info);

        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Outils", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Générer la grille", GUILayout.Height(28)))
                GenerateLayout(data);

            using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(data.finalWord)))
            {
                if (GUILayout.Button("Rechoisir les cases du mot final", GUILayout.Height(28)))
                    PickFinalWord(data, showMessage: true);
            }
        }

        if (!string.IsNullOrEmpty(lastMessage))
            EditorGUILayout.HelpBox(lastMessage, lastMessageType);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Vérifications", EditorStyles.boldLabel);

        GridInfo info = BuildGrid(data);
        DrawValidation(data, info);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Aperçu", EditorStyles.boldLabel);
        DrawPreview(info);
    }

    // ───────────────────────── Génération ─────────────────────────

    private void GenerateLayout(CrosswordData data)
    {
        if (data.words == null || data.words.Count == 0)
        {
            SetMessage("Ajoute d'abord des mots dans la liste « Words ».", MessageType.Warning);
            return;
        }

        Undo.RecordObject(data, "Générer la grille de mots croisés");

        // Mots en majuscules, sans espaces autour (les comparaisons en jeu ignorent déjà
        // accents et casse, mais la grille est plus lisible ainsi).
        foreach (WordData word in data.words)
            word.word = (word.word ?? string.Empty).Trim().ToUpperInvariant();

        if (!string.IsNullOrEmpty(data.finalWord))
            data.finalWord = data.finalWord.Trim().ToUpperInvariant();

        List<string> normalizedWords = data.words.Select(w => NormalizeWord(w.word)).ToList();

        string invalid = data.words.Select(w => w.word).FirstOrDefault(w => w.Length < 2 || w.Any(c => !char.IsLetter(c)));

        if (invalid != null)
        {
            SetMessage($"« {invalid} » n'est pas valide : un mot doit avoir au moins 2 lettres, sans espace ni chiffre.", MessageType.Error);
            return;
        }

        CrosswordLayoutGenerator.Result result =
            CrosswordLayoutGenerator.Generate(normalizedWords, GenerationAttempts, new System.Random());

        List<string> notPlaced = new();

        for (int i = 0; i < data.words.Count; i++)
        {
            CrosswordLayoutGenerator.Placement? placement = result.Placements[i];

            if (placement == null)
            {
                notPlaced.Add(data.words[i].word);
                continue;
            }

            data.words[i].originPosition = new Vector2Int(placement.Value.X, placement.Value.Y);
            data.words[i].isDown = placement.Value.Down;
        }

        // Mots non placés : retirés de la grille en jeu ? Non, on prévient seulement.
        bool finalOk = string.IsNullOrWhiteSpace(data.finalWord) || PickFinalWord(data, showMessage: false);

        EditorUtility.SetDirty(data);

        if (notPlaced.Count > 0)
        {
            SetMessage($"Impossible de croiser : {string.Join(", ", notPlaced)}. " +
                       "Ajoute un mot qui partage des lettres avec eux, ou retire-les. " +
                       "(Ils sont encore dans la liste et apparaîtraient au mauvais endroit en jeu.)",
                       MessageType.Error);
        }
        else if (!finalOk)
        {
            // Le message vient de PickFinalWord.
        }
        else
        {
            SetMessage($"Grille générée : {result.PlacedCount} mots, {result.Crossings} croisements. " +
                       "Re-clique pour une autre disposition.", MessageType.Info);
        }
    }

    private bool PickFinalWord(CrosswordData data, bool showMessage)
    {
        GridInfo info = BuildGrid(data);
        string finalWord = NormalizeWord(data.finalWord ?? string.Empty);

        Dictionary<(int, int), char> letters = info.Letters.ToDictionary(p => (p.Key.x, p.Key.y), p => p.Value);

        if (!CrosswordLayoutGenerator.PickFinalWordCells(letters, finalWord, new System.Random(),
                out List<(int, int)> cells, out char missing))
        {
            SetMessage($"Le mot final a besoin de la lettre « {missing} » une fois de plus qu'il n'y en a dans la grille. " +
                       "Ajoute un mot qui la contient, ou re-génère.", MessageType.Error);
            return false;
        }

        Undo.RecordObject(data, "Choisir les cases du mot final");
        data.finalWordLetterPositions = cells.Select(c => new Vector2Int(c.Item1, c.Item2)).ToList();
        EditorUtility.SetDirty(data);

        if (showMessage)
            SetMessage("Cases du mot final choisies.", MessageType.Info);

        return true;
    }

    private void SetMessage(string message, MessageType type)
    {
        lastMessage = message;
        lastMessageType = type;
    }

    private static string NormalizeWord(string word)
    {
        // Lettres seulement : « SAGRADA FAMILIA » → SAGRADAFAMILIA (l'espace ne sert qu'à l'affichage).
        return new string(word.Where(char.IsLetter).Select(CrosswordText.Normalize).ToArray());
    }

    // ───────────────────────── Grille & vérifications ─────────────────────────

    private class GridInfo
    {
        public readonly Dictionary<Vector2Int, char> Letters = new();
        public readonly HashSet<Vector2Int> Conflicts = new();
        public readonly Dictionary<Vector2Int, int> FinalIndex = new();
        public readonly Dictionary<Vector2Int, int> WordNumbers = new();
        public readonly List<string> AccidentalWords = new();
    }

    private static GridInfo BuildGrid(CrosswordData data)
    {
        GridInfo info = new GridInfo();

        if (data.words == null)
            return info;

        // Même numérotation qu'en jeu (CrosswordGridCreator) : de haut en bas, puis de gauche à droite.
        int number = 1;

        foreach (WordData word in data.words.Where(w => !string.IsNullOrEmpty(w.word))
                     .OrderBy(w => w.originPosition.y).ThenBy(w => w.originPosition.x))
        {
            if (!info.WordNumbers.ContainsKey(word.originPosition))
                info.WordNumbers[word.originPosition] = number++;
        }

        foreach (WordData word in data.words)
        {
            if (string.IsNullOrEmpty(word.word))
                continue;

            for (int i = 0; i < word.word.Length; i++)
            {
                Vector2Int position = word.originPosition + (word.isDown ? new Vector2Int(0, i) : new Vector2Int(i, 0));
                char letter = CrosswordText.Normalize(word.word[i]);

                if (info.Letters.TryGetValue(position, out char existing) && existing != letter)
                    info.Conflicts.Add(position);

                info.Letters[position] = letter;
            }
        }

        if (data.finalWordLetterPositions != null)
        {
            for (int i = 0; i < data.finalWordLetterPositions.Count; i++)
                info.FinalIndex[data.finalWordLetterPositions[i]] = i + 1;
        }

        // « Faux mots » : suites de lettres collées qui ne correspondent à aucun mot.
        HashSet<string> horizontalWords = new(data.words.Where(w => !w.isDown && !string.IsNullOrEmpty(w.word)).Select(w => NormalizeWord(w.word)));
        HashSet<string> verticalWords = new(data.words.Where(w => w.isDown && !string.IsNullOrEmpty(w.word)).Select(w => NormalizeWord(w.word)));

        FindRuns(info, Vector2Int.right, horizontalWords);
        FindRuns(info, Vector2Int.up, verticalWords);

        return info;
    }

    private static void FindRuns(GridInfo info, Vector2Int direction, HashSet<string> validWords)
    {
        foreach (Vector2Int start in info.Letters.Keys)
        {
            if (info.Letters.ContainsKey(start - direction))
                continue;

            string run = string.Empty;

            for (Vector2Int p = start; info.Letters.TryGetValue(p, out char c); p += direction)
                run += c;

            if (run.Length > 1 && !validWords.Contains(run))
                info.AccidentalWords.Add(run);
        }
    }

    private static void DrawValidation(CrosswordData data, GridInfo info)
    {
        bool ok = true;

        if (info.Conflicts.Count > 0)
        {
            ok = false;
            EditorGUILayout.HelpBox($"{info.Conflicts.Count} case(s) où deux mots demandent une lettre différente (en rouge dans l'aperçu).", MessageType.Error);
        }

        if (info.AccidentalWords.Count > 0)
        {
            ok = false;
            EditorGUILayout.HelpBox($"Mots collés qui forment des suites invalides : {string.Join(", ", info.AccidentalWords)}. " +
                                    "Le joueur ne pourra pas valider ces cases.", MessageType.Error);
        }

        string finalWord = NormalizeWord(data.finalWord ?? string.Empty);
        List<Vector2Int> positions = data.finalWordLetterPositions ?? new List<Vector2Int>();

        if (!string.IsNullOrEmpty(finalWord))
        {
            if (positions.Count != finalWord.Length)
            {
                ok = false;
                EditorGUILayout.HelpBox($"Le mot final a {finalWord.Length} lettres mais {positions.Count} case(s) sont choisies.", MessageType.Error);
            }
            else
            {
                string read = new string(positions.Select(p => info.Letters.TryGetValue(p, out char c) ? c : '?').ToArray());

                if (read != finalWord)
                {
                    ok = false;
                    EditorGUILayout.HelpBox($"Les cases du mot final donnent « {read} » au lieu de « {finalWord} ».", MessageType.Error);
                }

                if (positions.Distinct().Count() != positions.Count)
                {
                    ok = false;
                    EditorGUILayout.HelpBox("Une même case est utilisée deux fois dans le mot final.", MessageType.Error);
                }
            }
        }

        if (ok)
            EditorGUILayout.HelpBox("Grille valide ✔", MessageType.None);
    }

    // ───────────────────────── Aperçu ─────────────────────────

    private static void DrawPreview(GridInfo info)
    {
        if (info.Letters.Count == 0)
        {
            EditorGUILayout.LabelField("(grille vide)");
            return;
        }

        int minX = info.Letters.Keys.Min(p => p.x), maxX = info.Letters.Keys.Max(p => p.x);
        int minY = info.Letters.Keys.Min(p => p.y), maxY = info.Letters.Keys.Max(p => p.y);
        int width = maxX - minX + 1, height = maxY - minY + 1;

        Rect area = GUILayoutUtility.GetRect(width * CellSize, height * CellSize, GUILayout.ExpandWidth(false));

        GUIStyle letterStyle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };
        letterStyle.normal.textColor = Color.black;

        GUIStyle smallStyle = new GUIStyle(EditorStyles.miniLabel) { fontSize = 8 };
        smallStyle.normal.textColor = new Color(0.2f, 0.2f, 0.2f);

        GUIStyle finalStyle = new GUIStyle(smallStyle) { alignment = TextAnchor.LowerRight };
        finalStyle.normal.textColor = new Color(0.6f, 0.3f, 0f);

        EditorGUI.DrawRect(area, EmptyColor);

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                Vector2Int position = new(x, y);

                if (!info.Letters.TryGetValue(position, out char letter))
                    continue;

                Rect cell = new Rect(area.x + (x - minX) * CellSize + 1, area.y + (y - minY) * CellSize + 1, CellSize - 2, CellSize - 2);

                Color color = info.Conflicts.Contains(position) ? ConflictColor
                    : info.FinalIndex.ContainsKey(position) ? FinalColor
                    : CellColor;

                EditorGUI.DrawRect(cell, color);
                GUI.Label(cell, letter.ToString(), letterStyle);

                if (info.WordNumbers.TryGetValue(position, out int number))
                    GUI.Label(new Rect(cell.x + 1, cell.y - 2, cell.width, 10), number.ToString(), smallStyle);

                if (info.FinalIndex.TryGetValue(position, out int finalIndex))
                    GUI.Label(new Rect(cell.x, cell.y + 1, cell.width - 1, cell.height), finalIndex.ToString(), finalStyle);
            }
        }

        EditorGUILayout.LabelField("Jaune : lettres du mot final (avec leur numéro). Rouge : conflit.", EditorStyles.miniLabel);
    }
}
