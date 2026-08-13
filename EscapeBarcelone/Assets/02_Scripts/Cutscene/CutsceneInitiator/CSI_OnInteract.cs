using UnityEngine;

public class CSI_OnInteract : CutsceneInitiator
{
    [SerializeField] private InteractionElementBase interaction;

    protected override void Initialize()
    {
        base.Initialize();

        interaction.OnInteract += OnInteract;
    }

    private void OnInteract()
    {
        if (true)
        {
            interaction.OnInteract -= OnInteract;
        }

        StartCutscene();
    }
}
