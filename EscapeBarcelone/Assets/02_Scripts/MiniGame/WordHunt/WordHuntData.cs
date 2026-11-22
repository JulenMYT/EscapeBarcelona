using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Grille de la boussole : on y « sélectionne » un lieu (comme dans une recherche de mots)
/// pour s'y rendre. Chaque destination associe un mot de la grille à une scène.
/// La grille elle-même se génère depuis l'inspecteur (bouton « Générer la grille »).
/// </summary>
[CreateAssetMenu(fileName = "WordHuntData", menuName = "MiniGames/WordHunt")]
public class WordHuntData : ScriptableObject
{
    [Tooltip("Lieux qu'on peut sélectionner dans la grille.")]
    [SerializeField] private List<CompassDestination> destinations = new();

    // ── Génération automatique (utilisée seulement par l'inspecteur, voir WordHuntDataEditor) ──

    [Tooltip("Faux lieux cachés dans la grille pour brouiller les pistes (ex. MADRID, SEVILLA). " +
             "Les sélectionner ne fait rien.")]
    [SerializeField] private List<string> decoyWords = new();

    [SerializeField] private Vector2Int gridSize = new(15, 14);

    // Lus seulement par l'inspecteur (WordHuntDataEditor), pas en jeu.
#pragma warning disable CS0414
    [Tooltip("Autoriser les mots en diagonale.")]
    [SerializeField] private bool allowDiagonal = true;

    [Tooltip("Autoriser les mots écrits à l'envers (de droite à gauche, de bas en haut).")]
    [SerializeField] private bool allowReversed = false;
#pragma warning restore CS0414

    [Tooltip("Lignes de la grille, toutes de la même longueur. Remplies par « Générer la grille ».")]
    [SerializeField] private List<string> rows;

    public IReadOnlyList<string> Rows => rows;
    public IReadOnlyList<CompassDestination> Destinations => destinations;
}

[Serializable]
public class CompassDestination
{
    [Tooltip("Le mot à sélectionner dans la grille (accents et casse ignorés, dans les deux sens).")]
    public string word;

    [Tooltip("Scène chargée quand on choisit ce lieu. Elle doit être dans la liste des scènes du build.")]
    public string sceneName;

    [Tooltip("Flags nécessaires pour pouvoir y aller. Vide = toujours possible.")]
    public StoryCondition condition = new();

    [Tooltip("Optionnel : dialogue joué avant le départ (« En route pour… »).")]
    public Dialogue dialogueBeforeTravel;

    [Tooltip("Optionnel : dialogue joué si la condition n'est pas remplie (« Je n'ai rien à faire là-bas… »).")]
    public Dialogue lockedDialogue;

    [Tooltip("Optionnel : flag ajouté au départ.")]
    public StoryFlag flagOnTravel = StoryFlag.None;
}
