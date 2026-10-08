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
        Vector3 respawnPos = CheckpointManager.Instance.GetRespawnPoint(tag);

        transform.position = respawnPos;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        if (respawnVfx != null)
        {
            Instantiate(respawnVfx, respawnPos, Quaternion.identity);
        }
    }
}