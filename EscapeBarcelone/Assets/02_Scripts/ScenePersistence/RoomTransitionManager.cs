using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomTransitionManager : MonoBehaviour
{
    [SerializeField] private ScreenFader screenFader;

    private string currentRoom = "";
    private bool isTransitioning;

    private void Awake()
    {
        ServiceLocator.Register<RoomTransitionManager>(this);
    }

    void Start()
    {
        EnterRoom("");
    }

    public void EnterRoom(string sceneName)
    {
        if (isTransitioning)
            return;

        StartCoroutine(Transition(sceneName));
    }

    private IEnumerator Transition(string sceneName)
    {
        isTransitioning = true;    //Lock

        yield return screenFader.Fade(0f, 1f, 0.5f);

        if (!string.IsNullOrEmpty(currentRoom))
        {
            yield return SceneManager.UnloadSceneAsync(currentRoom);
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        }

        Scene newScene = SceneManager.GetSceneByName(sceneName);

        if (newScene.IsValid())
        {
            SceneManager.SetActiveScene(newScene);
        }

        currentRoom = SceneManager.GetActiveScene().name;

        yield return null;

        RoomService service = ServiceLocator.Get<RoomService>();
        ServiceLocator.Get<AudioManager>().PlayMusic(service.roomMusic);

        isTransitioning = false;

        yield return screenFader.Fade(1f, 0f, 0.5f);
    }
}

