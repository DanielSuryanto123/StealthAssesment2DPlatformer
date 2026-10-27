
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class PitfallTrigger : MonoBehaviour
{
    [SerializeField] private PitfallRescueManager rescueManager;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (rescueManager == null)
        {
            Debug.LogError("PitfallTrigger: Rescue Manager is not assigned.");
            return;
        }

        PlayerInput playerInput = other.GetComponentInParent<PlayerInput>();

        if (playerInput == null)
            return;

        GameObject player = playerInput.gameObject;

        if (player.CompareTag("Player2"))
            rescueManager.TrapPlayer(player);
    }
}
