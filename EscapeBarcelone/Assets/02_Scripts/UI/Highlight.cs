using UnityEngine;

public class Highlight : MonoBehaviour
{
    [SerializeField] private Transform highlightAnchor;
    [SerializeField] private Transform background;
    [SerializeField] private Transform defaultParent;

    public void StartHighlight()
    {
        background.SetParent(highlightAnchor);
    }

    public void StopHighlight()
    {
        background.SetParent(defaultParent);
    }
}