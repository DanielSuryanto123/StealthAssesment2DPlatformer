using UnityEngine;

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

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Respawn()
    {
        GetComponent<PlayerController>()?.ResetHazardState();

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