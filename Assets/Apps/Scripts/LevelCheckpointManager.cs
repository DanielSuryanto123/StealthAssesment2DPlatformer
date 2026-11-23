using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCheckpointManager : MonoBehaviour
{
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Ignore scenes before Level 1.
        if (scene.buildIndex < 1)
            return;

        if (GlobalGameTimer.Instance == null)
        {
            Debug.LogError("GlobalGameTimer not found!");
            return;
        }

        // Save the timer when entering a new level.
        GlobalGameTimer.Instance.SaveLevelCheckpoint();

        Debug.Log("Checkpoint saved for: " + scene.name);
    }
}
