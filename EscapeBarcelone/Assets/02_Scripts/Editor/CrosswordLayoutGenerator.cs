using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Calcule automatiquement la disposition d'une grille de mots croisés à partir d'une
/// simple liste de mots. Pur C# (aucune dépendance Unity) : utilisé par l'inspecteur
/// de CrosswordData.
///
/// Conventions (les mêmes que CrosswordGridCreator / CrosswordView) :
/// - horizontal : x augmente vers la droite ;
/// - vertical (isDown) : y augmente vers le BAS.
/// </summary>
public static class CrosswordLayoutGenerator
{
    public struct Placement
    {
        public int X;
        public int Y;
        public bool Down;
    }

    public class Result
    {
        /// <summary>Placement de chaque mot (même index que la liste d'entrée), null si non placé.</summary>
        public Placement?[] Placements;
        public int PlacedCount;
        public int Crossings;
        public int Area;
    }

    /// <summary>Essaie plusieurs dispositions et garde la meilleure.</summary>
    /// <param name="words">Mots en MAJUSCULES, sans espace.</param>
    public static Result Generate(IReadOnlyList<string> words, int attempts, Random random)
    {
        Result best = null;

        for (int attempt = 0; attempt < attempts; attempt++)
        {
            Result result = TryLayout(words, random, attempt == 0);

            if (best == null || IsBetter(result, best))
                best = result;

            // Impossible de faire mieux que tout placer de façon très compacte : on peut
            // s'arrêter, mais on laisse quelques essais pour varier.
        }

        return best;
    }

    private static bool IsBetter(Result a, Result b)
    {
        if (a.PlacedCount != b.PlacedCount)
            return a.PlacedCount > b.PlacedCount;

        if (a.Crossings != b.Crossings)
            return a.Crossings > b.Crossings;

        return a.Area < b.Area;
    }

    // ───────────────────────── Un essai ─────────────────────────

    private class Grid
    {
        public readonly Dictionary<(int, int), char> Letters = new();
        // Cases déjà utilisées par un mot horizontal / vertical.
        public readonly HashSet<(int, int)> UsedHorizontal = new();
        public readonly HashSet<(int, int)> UsedVertical = new();

        public int MinX, MaxX, MinY, MaxY;
        public bool Empty = true;

        public bool Has(int x, int y) => Letters.ContainsKey((x, y));

        public void Place(string word, Placement p)
        {
            for (int i = 0; i < word.Length; i++)
            {
                int x = p.X + (p.Down ? 0 : i);
                int y = p.Y + (p.Down ? i : 0);

                Letters[(x, y)] = word[i];
                (p.Down ? UsedVertical : UsedHorizontal).Add((x, y));

                if (Empty)
                {
                    MinX = MaxX = x;
                    MinY = MaxY = y;
                    Empty = false;
                }
                else
                {
                    MinX = Math.Min(MinX, x); MaxX = Math.Max(MaxX, x);
                    MinY = Math.Min(MinY, y); MaxY = Math.Max(MaxY, y);
                }
            }
        }
    }

    private static Result TryLayout(IReadOnlyList<string> words, Random random, bool firstAttempt)
    {
        // Ordre : les plus longs d'abord (ils offrent le plus de croisements), avec un peu
        // de hasard à partir du deuxième essai pour explorer d'autres dispositions.
        List<int> order = Enumerable.Range(0, words.Count)
            .OrderByDescending(i => words[i].Length + (firstAttempt ? 0 : random.NextDouble() * 3))
            .ToList();

        Grid grid = new Grid();
        Placement?[] placements = new Placement?[words.Count];
        int crossings = 0;
        int placed = 0;

        // Les mots impossibles à placer tout de suite sont retentés plus tard.
        List<int> pending = new List<int>(order);
        bool progress = true;

        while (pending.Count > 0 && progress)
        {
            progress = false;

            foreach (int index in pending.ToList())
            {
                string word = words[index];

                if (grid.Empty)
                {
                    Placement first = new Placement { X = 0, Y = 0, Down = random.Next(2) == 0 && !firstAttempt };
                    grid.Place(word, first);
                    placements[index] = first;
                    placed++;
                    pending.Remove(index);
                    progress = true;
                    continue;
                }

                if (TryFindBest(grid, word, random, out Placement best, out int bestCrossings))
                {
                    grid.Place(word, best);
                    placements[index] = best;
                    crossings += bestCrossings;
                    placed++;
                    pending.Remove(index);
                    progress = true;
                }
            }
        }

        return new Result
        {
            Placements = placements,
            PlacedCount = placed,
            Crossings = crossings,
            Area = grid.Empty ? 0 : (grid.MaxX - grid.MinX + 1) * (grid.MaxY - grid.MinY + 1)
        };
    }

    private static bool TryFindBest(Grid grid, string word, Random random, out Placement best, out int bestCrossings)
    {
        best = default;
        bestCrossings = 0;
        double bestScore = double.MinValue;
        bool found = false;

        // Candidats : chaque lettre du mot posée sur une lettre identique déjà dans la grille,
        // dans la direction perpendiculaire au mot déjà présent.
        foreach (KeyValuePair<(int, int), char> cell in grid.Letters.ToList())
        {
            for (int i = 0; i < word.Length; i++)
            {
                if (word[i] != cell.Value)
                    continue;

                (int cx, int cy) = cell.Key;

                foreach (bool down in new[] { false, true })
                {
                    // Le mot existant doit aller dans l'autre direction.
                    if (down && grid.UsedVertical.Contains(cell.Key)) continue;
                    if (!down && grid.UsedHorizontal.Contains(cell.Key)) continue;

                    Placement p = new Placement
                    {
                        X = down ? cx : cx - i,
                        Y = down ? cy - i : cy,
                        Down = down
                    };

                    if (!CanPlace(grid, word, p, out int crossingCount))
                        continue;

                    // Score : beaucoup de croisements, grille compacte et à peu près carrée.
                    int minX = Math.Min(grid.MinX, p.X), minY = Math.Min(grid.MinY, p.Y);
                    int maxX = Math.Max(grid.MaxX, p.X + (down ? 0 : word.Length - 1));
                    int maxY = Math.Max(grid.MaxY, p.Y + (down ? word.Length - 1 : 0));
                    int width = maxX - minX + 1, height = maxY - minY + 1;

                    double score = crossingCount * 10
                                   - width * height * 0.15
                                   - Math.Abs(width - height) * 0.5
                                   + random.NextDouble();   // départage les égalités

                    if (score > bestScore)
                    {
                        bestScore = score;
                        best = p;
                        bestCrossings = crossingCount;
                        found = true;
                    }
                }
            }
        }

        return found;
    }

    /// <summary>
    /// Règles classiques : pas de lettre en conflit, pas de mot collé parallèlement à un
    /// autre, cases vides juste avant et juste après le mot, au moins un croisement.
    /// </summary>
    private static bool CanPlace(Grid grid, string word, Placement p, out int crossingCount)
    {
        crossingCount = 0;

        int dx = p.Down ? 0 : 1, dy = p.Down ? 1 : 0;

        // Case avant le début et après la fin : vides (sinon le mot se prolonge).
        if (grid.Has(p.X - dx, p.Y - dy) || grid.Has(p.X + dx * word.Length, p.Y + dy * word.Length))
            return false;

        for (int i = 0; i < word.Length; i++)
        {
            int x = p.X + dx * i, y = p.Y + dy * i;

            if (grid.Letters.TryGetValue((x, y), out char existing))
            {
                if (existing != word[i])
                    return false;

                // Déjà utilisée dans la même direction : chevauchement, interdit.
                if (p.Down ? grid.UsedVertical.Contains((x, y)) : grid.UsedHorizontal.Contains((x, y)))
                    return false;

                crossingCount++;
                continue;
            }

            // Case nouvelle : ses voisines sur les côtés doivent être vides, sinon on créerait
            // des « faux mots » en collant ce mot contre un autre.
            if (grid.Has(x + dy, y + dx) || grid.Has(x - dy, y - dx))
                return false;
        }

        return crossingCount > 0;
    }

    // ───────────────────────── Mot final ─────────────────────────

    /// <summary>
    /// Choisit une case de la grille pour chaque lettre du mot final (une case différente
    /// par lettre), en essayant de les répartir dans toute la grille.
    /// Renvoie false et la première lettre introuvable si c'est impossible.
    /// </summary>
    public static bool PickFinalWordCells(Dictionary<(int, int), char> letters, string finalWord, Random random,
        out List<(int, int)> cells, out char missingLetter)
    {
        cells = new List<(int, int)>();
        missingLetter = '\0';
        HashSet<(int, int)> used = new();

        foreach (char letter in finalWord)
        {
            List<(int, int)> candidates = letters
                .Where(pair => pair.Value == letter && !used.Contains(pair.Key))
                .Select(pair => pair.Key)
                .ToList();

            if (candidates.Count == 0)
            {
                missingLetter = letter;
                return false;
            }

            // Préférer la case la plus éloignée de celles déjà choisies (répartition).
            (int, int) chosen = candidates
                .OrderByDescending(c => used.Count == 0 ? 0 : used.Min(u => Math.Abs(u.Item1 - c.Item1) + Math.Abs(u.Item2 - c.Item2)))
                .ThenBy(_ => random.Next())
                .First();

            used.Add(chosen);
            cells.Add(chosen);
        }

        return true;
    }
}
