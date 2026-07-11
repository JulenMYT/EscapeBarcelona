using UnityEngine;

public class IE_ZoomIn : InteractionElementBase
{
    [SerializeField] protected InteractionElementBase targetObject;
    [SerializeField] private int layoutOrderAfter;
    [SerializeField] private int layoutOrderBefore;
    [SerializeField] private bool hideOnZoom;

    private FocusPanelGameplay focusPanel;

    public override void Initialize()
    {
        focusPanel = FindAnyObjectByType<FocusPanelGameplay>();
    }

    protected virtual void BeforeOpenTarget()
    {

    }

    public override void Interact()
    {
        base.Interact();

        Exit();

        PushFocus(targetObject.transform);

        focusPanel.SetOrderInLayer(layoutOrderAfter);
        focusPanel.Show();

        BeforeOpenTarget();

        targetObject.OnClose += HandleClose;
        targetObject.Open();

        if (hideOnZoom)
            gameObject.SetActive(false);
    }

    private void HandleClose()
    {
        PopFocus();

        targetObject.OnClose -= HandleClose;

        focusPanel.SetOrderInLayer(layoutOrderBefore);

        if (hideOnZoom)
            gameObject.SetActive(true);
    }

    public static void PushFocus(Transform target)
    {
        focusStack.Push(target);
    }

    public static void PopFocus()
    {
        if (focusStack.Count > 0)
            focusStack.Pop();
    }
}