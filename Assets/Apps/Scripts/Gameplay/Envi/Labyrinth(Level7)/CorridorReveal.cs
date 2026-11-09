
using UnityEngine;

public class CorridorReveal : MonoBehaviour
{
    [SerializeField] private GameObject darkness;
    [SerializeField] private bool revealForBothPlayers = true;
    [SerializeField] private string requiredPlayerTag = "Player2";

    private void Start()
    {
        if (darkness != null)
            darkness.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        bool isPlayer =
            other.CompareTag("Player1") ||
            other.CompareTag("Player2");

        if (!isPlayer)
            return;

        if (!revealForBothPlayers &&
            !other.CompareTag(requiredPlayerTag))
            return;

        if (darkness != null)
            darkness.SetActive(false);
    }
}
