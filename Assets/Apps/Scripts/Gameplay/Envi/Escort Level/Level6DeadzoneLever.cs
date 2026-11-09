using UnityEngine;
using CoLab.Telemetry;

public class Level6DeadzoneLever : MonoBehaviour
{
    [Header("Rescue")]
    [SerializeField] private Level6DeadzoneTrigger targetDeadzone;
    [SerializeField] private bool singleUse = true;

    private bool isActivated;

    public void ActivateLever(Transform actor)
    {
        if (actor == null || !actor.CompareTag("Player2"))
            return;

        if (singleUse && isActivated)
            return;

        if (targetDeadzone == null)
        {
            Debug.LogWarning("Target Deadzone has not been assigned.");
            return;
        }

        isActivated = true;

        targetDeadzone.ReleasePlayer();

        if (AnalyticsManager.Instance != null)
        {
            AnalyticsManager.Instance.LogLeverPull(transform);
        }

        Debug.Log(gameObject.name + " activated by Player2.");
    }

    public void ResetLever()
    {
        isActivated = false;
    }
}