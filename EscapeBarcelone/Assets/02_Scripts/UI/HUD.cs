using UnityEngine;

public class HUD : MonoBehaviour
{
    [Tooltip("Bouton d'ouverture de la boussole (caché tant qu'elle n'est pas débloquée).")]
    [SerializeField] private GameObject compass;
    [Tooltip("Bouton d'ouverture du carnet (caché tant qu'il n'est pas débloqué).")]
    [SerializeField] private GameObject book;

    private WorldState worldState;

    private void Start()
    {
        worldState = ServiceLocator.Get<WorldState>();
        worldState.OnStoryFlagAdded += HandleNewFlag;

        // Si les flags existent déjà (sauvegarde, test…), on débloque tout de suite.
        if (worldState.HasFlag(StoryFlag.HasCompass))
            UnlockCompass();

        if (worldState.HasFlag(StoryFlag.HasBook))
            UnlockBook();
    }

    private void OnDestroy()
    {
        if (worldState != null)
            worldState.OnStoryFlagAdded -= HandleNewFlag;
    }

    public void HandleNewFlag(StoryFlag flag)
    {
        if (flag == StoryFlag.HasCompass)
        {
            UnlockCompass();
        }
        else if (flag == StoryFlag.HasBook)
        {
            UnlockBook();
        }
    }

    public void UnlockCompass()
    {
        // Si la boussole est déjà ouverte, le bouton d'ouverture doit rester caché.
        Compass compassPanel = ServiceLocator.Get<Compass>();

        if (compassPanel == null || !compassPanel.IsOpen)
            compass.SetActive(true);
    }

    public void UnlockBook()
    {
        Book bookPanel = ServiceLocator.Get<Book>();

        if (bookPanel == null || !bookPanel.IsOpen)
            book.SetActive(true);
    }
}
