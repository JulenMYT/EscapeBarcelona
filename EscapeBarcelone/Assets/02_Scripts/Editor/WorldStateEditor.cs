using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// En Play, affiche tous les StoryFlags dans l'inspecteur de WorldState, avec une case
/// à cocher pour chacun : on voit l'avancée de l'histoire en direct et on peut
/// ajouter / retirer un flag pour tester.
/// </summary>
[CustomEditor(typeof(WorldState))]
public class WorldStateEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Story flags (en direct)", EditorStyles.boldLabel);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Lance le Play pour voir et modifier les flags en direct.", MessageType.Info);
            return;
        }

        WorldState worldState = (WorldState)target;

        foreach (StoryFlag flag in Enum.GetValues(typeof(StoryFlag)))
        {
            if (flag == StoryFlag.None)
                continue;

            bool hasFlag = worldState.HasFlag(flag);
            bool newValue = EditorGUILayout.ToggleLeft($"{flag}  ({(int)flag})", hasFlag);

            if (newValue == hasFlag)
                continue;

            if (newValue)
                worldState.AddFlag(flag);
            else
                worldState.RemoveFlag(flag);
        }
    }

    // Rafraîchit l'affichage en Play, pour voir les flags ajoutés par les cutscenes.
    public override bool RequiresConstantRepaint() => Application.isPlaying;
}
