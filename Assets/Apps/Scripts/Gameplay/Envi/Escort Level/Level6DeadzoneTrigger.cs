using UnityEngine;
using CoLab.Telemetry;

public class Level6DeadzoneTrigger : MonoBehaviour
{
    [Header("Deadzone")]
    [SerializeField] private string playerTag = "Player1";
    [SerializeField] private string deadzoneName = "Deadzone_1";

    private PlayerController trappedPlayer;
    private Rigidbody2D trappedRb;
    private float trappedStartTime;
    private bool isTrapped;

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
        trappedPlayer.enabled = false;

        if (AnalyticsManager.Instance != null)
        {
            AnalyticsManager.Instance.LogHazardEntered(
                other.transform,
                true
            );
        }

        Debug.Log(other.name + " is trapped at " + deadzoneName);
    }

    public void ReleasePlayer()
    {
        if (!isTrapped)
            return;

        float rescueDuration = Time.time - trappedStartTime;

        if (trappedRb != null)
            trappedRb.linearVelocity = Vector2.zero;

        if (trappedPlayer != null)
            trappedPlayer.enabled = true;

        Debug.Log(
            "Player rescued from " + deadzoneName +
            ". Rescue duration: " + rescueDuration + " seconds."
        );

        isTrapped = false;
        trappedPlayer = null;
        trappedRb = null;
    }
}