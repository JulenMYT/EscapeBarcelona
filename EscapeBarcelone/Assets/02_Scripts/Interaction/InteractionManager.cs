using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public bool IsLocked { get; private set; }

    private readonly HashSet<GameObject> allowedObjects = new();

    private void Awake()
    {
        ServiceLocator.Register<InteractionManager>(this);
    }

    public void Lock()
    {
        IsLocked = true;
        allowedObjects.Clear();
    }

    public void Unlock()
    {
        IsLocked = false;
        allowedObjects.Clear();
    }

    public void AllowObject(GameObject obj)
    {
        allowedObjects.Add(obj);
    }

    public void DisallowObject(GameObject obj)
    {
        allowedObjects.Remove(obj);
    }

    public bool CanInteract(GameObject obj)
    {
        return !IsLocked || allowedObjects.Contains(obj);
    }
}