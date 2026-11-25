using UnityEngine;
using UnityEngine.SceneManagement;

public class TimerResultHandler : MonoBehaviour
{
    [SerializeField] private string resultSceneName = "Result";

    private bool hasLoadedResult;

    private void Start()
    {
        if (GlobalGameTimer.Instance == null)
        {
            Debug.LogError("TimerResultHandler: GlobalGameTimer instance not found!");
            return;
        }

        GlobalGameTimer.Instance.OnTimeExpired += HandleTimeExpired;

        // Also handle the case where the timer already expired.
        if (GlobalGameTimer.Instance.IsExpired)
        {
            HandleTimeExpired();
        }
    }

    private void OnDestroy()
    {
        if (GlobalGameTimer.Instance != null)
        {
            GlobalGameTimer.Instance.OnTimeExpired -= HandleTimeExpired;
        }
    }

    private void HandleTimeExpired()
    {
        if (hasLoadedResult)
            return;

        hasLoadedResult = true;

        Debug.Log("TIME EXPIRED! Loading Result scene.");

        SceneManager.LoadScene(resultSceneName);
    }
}
