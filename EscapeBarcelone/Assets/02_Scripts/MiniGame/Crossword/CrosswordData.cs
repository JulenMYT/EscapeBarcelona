using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CrosswordData", menuName = "MiniGames/CrosswordData")]
public class CrosswordData : ScriptableObject
{
    public List<WordData> words;
    [Tooltip("Mot à trouver avec les cases illuminées. Un espace (« SAGRADA FAMILIA ») laisse une case vide entre les mots.")]
    public string finalWord;
    public List<Vector2Int> finalWordLetterPositions;
}

[Serializable]
public class WordData
{
    public string word;

    [TextArea]
    public string hint;

    public Vector2Int originPosition;
    public bool isDown;

    [Tooltip("Mot déjà écrit dans la grille au début (ex. ESPANA, entouré dans le guide), pour montrer au joueur comment ça marche.")]
    public bool prefilled;

    [NonSerialized] public int Number;
}