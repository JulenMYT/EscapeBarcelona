using UnityEngine;

public class Highlight : MonoBehaviour
{
    [SerializeField] private GameObject highlightCircle;
    [SerializeField] private Transform background;
    [SerializeField] private Transform defaultParent;

    public void StartHighlight()
    {
        background.SetParent(highlightCircle.transform);
    }

    public void StopHighlight()
    {
        background.SetParent(defaultParent);
    }
}