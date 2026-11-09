using UnityEngine;

/// <summary>
/// A checkpoint trigger. When a player touches it, it becomes that
/// player's respawn point. Works per-player (via tag), so Player1 and
/// Player2 can be at different checkpoints if they're in different areas.
///
/// Setup:
/// - Attach to a GameObject with a Collider2D set to "Is Trigger".
/// - Place at safe spots throughout the level.
/// </summary>
public class Checkpoint : MonoBehaviour
{
    [Tooltip("Optional: exact point to respawn at. If empty, uses this GameObject's position.")]
    [SerializeField] private Transform respawnPoint;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player1") && !other.CompareTag("Player2")) return;

        Transform point = respawnPoint != null ? respawnPoint : transform;
        CheckpointManager.Instance.SetCheckpoint(other.tag, point.position);
    }
}