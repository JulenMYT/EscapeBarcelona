using UnityEngine;

/// <summary>
/// Mot de passe trouvé : joue l'animation d'ouverture du coffre, puis la cutscene.
/// Les entrées sont bloquées dès la réussite (pas seulement à partir de la cutscene),
/// et le coffre reste ouvert quand on rouvre le gros plan ou qu'on revient dans la salle.
/// </summary>
public class CutsceneTriggerPasswordMiniGame : CutsceneTriggerMiniGame
{
    private const string OpenTrigger = "open";
    private const string OpenState = "Chest_Open";

    [SerializeField] private Animator anim;

    private bool waitingForAnimation;

    private void Awake()
    {
        // Sans ça, l'Animator revient à l'état fermé à chaque réouverture du gros plan.
        anim.keepAnimatorStateOnDisable = true;
    }

    private void Start()
    {
        // Retour dans la salle après la réussite : coffre directement ouvert, sans cutscene.
        if (miniGame.IsCompleted && !waitingForAnimation)
            anim.Play(OpenState, 0, 1f);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        // Gros plan fermé ou salle déchargée pendant l'animation : on ne laisse pas le jeu bloqué.
        if (waitingForAnimation)
        {
            waitingForAnimation = false;
            CutsceneDirector.Instance?.Release(this);
        }
    }

    protected override void OnGameCompleted()
    {
        waitingForAnimation = true;

        // Plus aucune entrée pendant l'animation : même blocage que pendant la cutscene.
        CutsceneDirector.Instance.Hold(this);

        anim.SetTrigger(OpenTrigger);
    }

    /// <summary>Appelé par l'événement placé à la fin de l'animation Chest_Open.</summary>
    public void AnimationFinished()
    {
        // L'événement peut aussi se déclencher quand on remet le coffre ouvert (Start).
        if (!waitingForAnimation)
            return;

        waitingForAnimation = false;

        cutscene.StartCutscene();
        CutsceneDirector.Instance.Release(this);
    }
}
