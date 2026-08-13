using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    private bool isLocked;
    private GameObject currentInspection;

    private readonly HashSet<GameObject> allowedObjects = new();

    private void Awake()
    {
        ServiceLocator.Register<InteractionManager>(this);
    }

    public bool CanInteract(GameObject obj)
    {
        if (isLocked)
            return allowedObjects.Contains(obj);

        if (currentInspection != null)
            return obj.transform.IsChildOf(currentInspection.transform);

        return true;
    }

    public void Lock()
    {
        isLocked = true;
        allowedObjects.Clear();
    }

    public void Unlock()
    {
        isLocked = false;
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

    public void SetCurrentInspection(GameObject inspection)
    {
        currentInspection = inspection;
    }

    public void ClearCurrentInspection()
    {
        currentInspection = null;
    }
}