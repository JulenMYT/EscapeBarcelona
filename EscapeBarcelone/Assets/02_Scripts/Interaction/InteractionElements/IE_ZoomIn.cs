using UnityEngine;

public class IE_ZoomIn : InteractionElementBase
{
    [SerializeField] protected InteractionElementBase targetObject;
    [SerializeField] private int layoutOrderAfter;
    [SerializeField] private int layoutOrderBefore;
    [SerializeField] private bool hideOnZoom;

    private FocusPanel focusPanel;

    public override void Initialize()
    {
        focusPanel = FindAnyObjectByType<FocusPanel>();
    }

    private void HandleClose()
    {
        targetObject.OnClose -= HandleClose;

        focusPanel.SetOrderInLayer(layoutOrderBefore);

        if (hideOnZoom)
            gameObject.SetActive(true);
    }

    protected virtual void BeforeOpenTarget()
    {

    }

    public override void Interact()
    {
        base.Interact();

        focusPanel.SetOrderInLayer(layoutOrderAfter);
        focusPanel.Show();

        BeforeOpenTarget();

        targetObject.OnClose += HandleClose;
        targetObject.Open();

        if (hideOnZoom)
            gameObject.SetActive(false);
    }
}