using System.Collections.Generic;
using UnityEngine;

public class CutsceneTriggerOpenBook : MonoBehaviour
{
    [SerializeField] private CutsceneInitiator cutscene;
    [SerializeField] private List<StoryFlag> requiredFlags;
    [SerializeField] private List<StoryFlag> forbiddenFlags;

    private Book book;

    private void Start()
    {
        book = ServiceLocator.Get<Book>();

        if (book != null)
            book.OnBookOpened += HandleBookOpened;
    }

    // Le carnet vit dans Bootstrap : sans ce désabonnement, il appellerait
    // encore ce script après le déchargement de la salle.
    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void HandleBookOpened()
    {
        if (!CanPlay())
            return;

        Unsubscribe();
        cutscene.StartCutscene();
    }

    private bool CanPlay() => StoryCondition.Check(requiredFlags, forbiddenFlags);

    private void Unsubscribe()
    {
        if (book != null)
            book.OnBookOpened -= HandleBookOpened;

        book = null;
    }
}
