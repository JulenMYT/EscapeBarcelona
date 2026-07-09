using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FocusPanel : Panel
{
    private Image darkImage;
    private Material darkImageMaterial;

    private Dictionary<SpriteRenderer, int> previousOrders = new();

    private const int HighlightOffset = 50;

    protected override void Initialize()
    {
        base.Initialize();

        Hide();
        darkImage = GetComponentInChildren<Image>();
        darkImageMaterial = darkImage.material;
    }

    public void SetMaterialOffset(Vector2 offset)
    {
        darkImageMaterial.SetVector("_Offset", offset);
    }

    public void SetMaterialSizeX(float sizeX)
    {
        darkImageMaterial.SetFloat("_SizeX", sizeX);
    }

    public void SetMaterialSizeY(float sizeY)
    {
        darkImageMaterial.SetFloat("_SizeY", sizeY);
    }

    public void SetMaterialSize(Vector2 size)
    {
        SetMaterialSizeX(size.x);
        SetMaterialSizeY(size.y);
    }

    public int GetOrderInLayer()
    {
        return darkImage.canvas.sortingOrder;
    }

    public void SetOrderInLayer(int order)
    {
        darkImage.canvas.sortingOrder = order;
    }

    public void Highlight(SpriteRenderer[] renderers)
    {
        Unhighlight();

        foreach (var renderer in renderers)
        {
            previousOrders.Add(renderer, renderer.sortingOrder);
            renderer.sortingOrder += HighlightOffset;
        }
    }

    public void Unhighlight()
    {
        foreach (var pair in previousOrders)
        {
            pair.Key.sortingOrder = pair.Value;
        }

        previousOrders.Clear();
    }

    public override void Show()
    {
        canvasGroup.alpha = 1.0f;
        SetMaterialOffset(new Vector2(10, 0));
    }
}