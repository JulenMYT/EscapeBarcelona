using UnityEngine;

public class FocusPanelGameplay : Panel
{
    private Canvas canvas;

    protected override void Initialize()
    {
        base.Initialize();

        canvas = GetComponentInParent<Canvas>();
        Hide();
    }

    public int GetOrderInLayer()
    {
        return canvas.sortingOrder;
    }

    public void SetOrderInLayer(int order)
    {
        canvas.sortingOrder = order;
    }
}
