using UnityEngine;

public class UniversalLevelRestart : MonoBehaviour
{
    [SerializeField] private KeyCode restartKey = KeyCode.R;

    private bool isRestarting;

    private void Update()
    {
        if (!Input.GetKeyDown(restartKey) || isRestarting)
            return;

        if (GlobalGameTimer.Instance == null)
        {
            Debug.LogError("GlobalGameTimer not found!");
            return;
        }

        isRestarting = true;

        // Restore timer before reloading the scene.
        GlobalGameTimer.Instance.RestoreLevelCheckpoint();

        // Reload the current level.
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }
}
