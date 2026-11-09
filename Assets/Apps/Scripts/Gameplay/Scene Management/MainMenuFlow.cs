using CoLab.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

// Bridges the main menu (CoLab.UI asmdef, cannot reference Assembly-CSharp) to SceneLoader:
// once PlayerIDWidget reports a started session, load Level1.
public static class MainMenuFlow
{
    private const string MenuSceneName = "MainMenu";
    private const string FirstLevelSceneName = "Level1";

    private static PlayerIDWidget _widget;
    private static bool _loading;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;

        // The first scene is already loaded when this runs.
        Attach(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Attach(scene);
    }

    private static void OnSceneUnloaded(Scene scene)
    {
        if (scene.name == MenuSceneName)
        {
            Detach();
        }
    }

    private static void Attach(Scene scene)
    {
        if (scene.name != MenuSceneName)
        {
            return;
        }

        Detach();
        _loading = false;

        _widget = Object.FindAnyObjectByType<PlayerIDWidget>(FindObjectsInactive.Include);
        if (_widget != null)
        {
            _widget.SessionStarted += OnSessionStarted;
        }
    }

    private static void Detach()
    {
        if (_widget != null)
        {
            _widget.SessionStarted -= OnSessionStarted;
        }
        _widget = null;
    }

    private static void OnSessionStarted()
    {
        if (_loading)
        {
            return;
        }
        _loading = true;

        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.ChangeScene(FirstLevelSceneName);
        }
        else
        {
            SceneManager.LoadScene(FirstLevelSceneName);
        }
    }
}
