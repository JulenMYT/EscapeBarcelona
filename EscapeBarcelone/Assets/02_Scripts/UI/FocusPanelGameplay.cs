using UnityEngine;

public class FocusPanelGameplay : Panel
{
    private Canvas canvas;

    public int GetOrderInLayer()
    {
        return canvas.sortingOrder;
    }

    public void SetOrderInLayer(int order)
    {
        canvas.sortingOrder = order;
    }
}
