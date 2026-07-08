using UnityEngine;
using UnityEngine.UI;

public class FocusPanel : Panel
{
    private Image darkImage;
    private Material darkImageMaterial;

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

    public override void Show()
    {
        canvasGroup.alpha = 1.0f;
        SetMaterialOffset(new Vector2(10,0));
    }
}
