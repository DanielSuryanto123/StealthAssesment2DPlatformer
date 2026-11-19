using UnityEngine;

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

        Transform playerRoot = other.transform.root;

        if (!playerRoot.CompareTag("Player2"))
            return;

        rescueManager.TrapPlayer(playerRoot.gameObject);
    }
}
