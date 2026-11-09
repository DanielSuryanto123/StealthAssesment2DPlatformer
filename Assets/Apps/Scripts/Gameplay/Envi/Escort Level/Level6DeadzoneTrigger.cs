using UnityEngine;
using CoLab.Telemetry;
using EventType = CoLab.Telemetry.EventType;

public class Level6DeadzoneTrigger : MonoBehaviour
{
    [Header("Deadzone")]
    [SerializeField] private string playerTag = "Player1";
    [SerializeField] private string deadzoneName = "Deadzone_1";

    private PlayerController trappedPlayer;
    private Rigidbody2D trappedRb;
    private float trappedStartTime;
    private bool isTrapped;
    private PlayerID trappedId;
    private bool hasTrappedId;

    public bool IsTrapped => isTrapped;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTrapped || !other.CompareTag(playerTag))
            return;

        trappedPlayer = other.GetComponent<PlayerController>();
        trappedRb = other.GetComponent<Rigidbody2D>();

        if (trappedPlayer == null || trappedRb == null)
        {
            Debug.LogWarning("PlayerController or Rigidbody2D missing.");
            return;
        }

        isTrapped = true;
        trappedStartTime = Time.time;

        trappedRb.linearVelocity = Vector2.zero;

        // Must run before the controller is disabled so a jump in progress resolves as JUMP_FAIL.
        trappedPlayer.NotifyHazardEntered();
        trappedPlayer.enabled = false;

        if (AnalyticsManager.Instance != null)
        {
            AnalyticsManager.Instance.LogHazardEntered(
                other.transform,
                true
            );

            AnalyticsManager.Instance.LogEvent(
                other.transform,
                EventType.RESCUE_START,
                true
            );

            if (AnalyticsManager.TryGetPlayerID(other.transform, out PlayerID victimId))
            {
                trappedId = victimId;
                hasTrappedId = true;
                AnalyticsManager.Instance.SetPlayerTrapped(victimId, true);
            }
        }

        Debug.Log(other.name + " is trapped at " + deadzoneName);
    }

    public void ReleasePlayer(Transform rescuer = null)
    {
        if (!isTrapped)
            return;

        float rescueDuration = Time.time - trappedStartTime;

        if (trappedRb != null)
            trappedRb.linearVelocity = Vector2.zero;

        if (trappedPlayer != null)
            trappedPlayer.enabled = true;

        if (rescuer != null)
        {
            AnalyticsManager.Instance?.LogEvent(
                rescuer,
                EventType.RESCUE_TIME,
                false,
                rescueDuration
            );
        }
        else
        {
            Debug.LogWarning(deadzoneName + ": ReleasePlayer called without a rescuer; RESCUE_TIME not logged.");
        }

        if (hasTrappedId)
            AnalyticsManager.Instance?.SetPlayerTrapped(trappedId, false);

        Debug.Log(
            "Player rescued from " + deadzoneName +
            ". Rescue duration: " + rescueDuration + " seconds."
        );

        isTrapped = false;
        trappedPlayer = null;
        trappedRb = null;
        hasTrappedId = false;
    }
}