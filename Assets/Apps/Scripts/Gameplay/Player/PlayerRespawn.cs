using UnityEngine;
using CoLab.Telemetry;
using EventType = CoLab.Telemetry.EventType;

/// <summary>
/// Handles moving this player back to their last checkpoint after a
/// hazard hit. No lives/health system — every hazard hit is an instant
/// respawn, with the level timer as the only real penalty.
///
/// Setup:
/// - Attach to each Player GameObject (alongside PlayerController).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerRespawn : MonoBehaviour
{
    [SerializeField] private GameObject respawnVfx;

    private Rigidbody2D rb;
    private float lastHazardTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>Remembers when the hazard hit happened, so Respawn can log how long the player was down.</summary>
    public void NotifyHazard()
    {
        lastHazardTime = Time.time;
    }

    public void Respawn()
    {
        AnalyticsManager.Instance?.LogEvent(transform, EventType.LEVEL_RESTART, true, Mathf.Max(0f, Time.time - lastHazardTime));
        GetComponent<PlayerController>()?.ResetJumpState();

        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        Vector3 respawnPos = CheckpointManager.Instance != null
            ? CheckpointManager.Instance.GetRespawnPoint(tag)
            : transform.position;

        transform.position = respawnPos;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (respawnVfx != null)
        {
            Instantiate(respawnVfx, respawnPos, Quaternion.identity);
        }
    }
}