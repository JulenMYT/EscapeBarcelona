using System.Collections;
using System.Linq;
using UnityEngine;

public class CSE_TutorialStep : CutsceneElementBase
{
    [Header("Highlight")]
    [SerializeField] private GameObject[] objectsToHighlight;

    [Header("Focus")]
    [SerializeField] private Vector2 size;
    [SerializeField] private Vector2 offset;

    [Header("ToggleObect")]
    [SerializeField] private GameObject target;
    [SerializeField] private bool updateTransform;

    [SerializeField] private Vector3 position;
    [SerializeField] private Vector3 rotation;

    [Header("Tutorial")]
    [SerializeField] private Tutorial tutorial;
    [SerializeField] private Panel panel;

    [Header("WaitForClick")]
    [SerializeField] private InteractionElementBase interactionElement;


    public override void Execute()
    {
        base.Execute();

        FocusPanelTutorial focusPanel = cutsceneHandler.focusPanel;

        focusPanel.Show();
        focusPanel.SetMaterialOffset(offset);
        focusPanel.SetMaterialSize(size);

        SpriteRenderer[] renderers = objectsToHighlight
            .SelectMany(x => x.GetComponentsInChildren<SpriteRenderer>())
            .ToArray();

        focusPanel.Highlight(renderers);

        if (target)
        {
            target.SetActive(true);

            if (updateTransform)
            {
                target.transform.position = position;
                target.transform.rotation = Quaternion.Euler(rotation);
            }
        }

        cutsceneHandler.tutorialHandler.StartTutorial(tutorial);

        interactionElement.OnInteract += OnInteraction;
        StartCoroutine(WaitBeforeInteraction());
    }

    private IEnumerator WaitBeforeInteraction()
    {
        yield return new WaitForSeconds(duration);
        interactionElement.SetIgnoreInteractionBlock(true);
    }

    private void OnInteraction()
    {
        interactionElement.OnInteract -= OnInteraction;
        interactionElement.SetIgnoreInteractionBlock(false);

        cutsceneHandler.focusPanel.Unhighlight();
        cutsceneHandler.focusPanel.Hide();

        if (target)
        {
            target.SetActive(false);
        }

        FocusPanelTutorial focusPanel = cutsceneHandler.focusPanel;

        focusPanel.Hide();
        panel.Hide();

        cutsceneHandler.PlayNextElement();
    }
}