using UnityEngine;

public class IE_ZoomIn : InteractionElementBase
{
    [SerializeField] private InteractionElementBase targetObject;
    [SerializeField] private int layoutOrderAfter;
    private int layoutOrderBefore;
    [SerializeField] private bool hideOnZoom;

    private FocusPanel focusPanel;

    public override void Initialize()
    {
        focusPanel = FindAnyObjectByType<FocusPanel>();
        targetObject.OnClose += HandleClose;
    }

    private void HandleClose()
    {
        targetObject.OnClose -= HandleClose;

        focusPanel.SetOrderInLayer(layoutOrderBefore);

        if (hideOnZoom)
            gameObject.SetActive(true);
    }

    public override void Interact()
    {
        base.Interact();
        layoutOrderBefore = focusPanel.GetOrderInLayer();

        focusPanel.SetOrderInLayer(layoutOrderAfter);
        focusPanel.Show();

        targetObject.OnClose += HandleClose;
        targetObject.Open();

        if (hideOnZoom)
            gameObject.SetActive(false);
    }
}
