using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Scenes", menuName = "Scriptable Objects/Scenes")]
public class Scenes : ScriptableObject
{
    public SceneData[] scenes;

    [Serializable]
    public class SceneData
    {
        public SceneName name;
        public Vector2 cameraPosition;
    }
}