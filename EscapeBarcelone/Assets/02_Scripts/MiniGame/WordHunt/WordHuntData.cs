using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Grille de la boussole : on y « sélectionne » un lieu (comme dans une recherche de mots)
/// pour s'y rendre. Chaque destination associe un mot de la grille à une scène.
/// </summary>
[CreateAssetMenu(fileName = "WordHuntData", menuName = "MiniGames/WordHunt")]
public class WordHuntData : ScriptableObject
{
    [Tooltip("Lignes de la grille, toutes de la même longueur.")]
    [SerializeField] private List<string> rows;

    [Tooltip("Lieux qu'on peut sélectionner dans la grille.")]
    [SerializeField] private List<CompassDestination> destinations = new();

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
