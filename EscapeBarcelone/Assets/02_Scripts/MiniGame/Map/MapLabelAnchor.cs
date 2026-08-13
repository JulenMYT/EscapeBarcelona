using UnityEngine;

public class MapLabelAnchor : MonoBehaviour
{
    [SerializeField] private MapLabel expectedLabel;

    private MapLabel currentLabel;

    public bool TryPlace(MapLabel label)
    {
        if (currentLabel != null)
            return false;

        currentLabel = label;
        label.Anchor(this);

        return true;
    }

    public void RemoveLabel()
    {
        currentLabel = null;
    }

    public bool IsOccupied => currentLabel != null;

    public bool IsCorrect()
    {
        return currentLabel == expectedLabel;
    }
}