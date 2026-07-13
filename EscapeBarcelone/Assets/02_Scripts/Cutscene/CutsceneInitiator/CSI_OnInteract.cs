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
        if (playOnce)
        {
            interaction.OnInteract -= OnInteract;
        }

        StartCutscene();
    }
}
