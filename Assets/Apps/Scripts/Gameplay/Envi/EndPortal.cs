using CoLab.Telemetry;
using UnityEngine;
using UnityEngine.SceneManagement;
using EventType = CoLab.Telemetry.EventType;

public class EndPortal : MonoBehaviour, ITelemetryTarget
{
    private bool player1Inside;
    private bool player2Inside;
    private bool isLoading;
    private int player1Contacts;
    private int player2Contacts;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player1"))
        {
            player1Inside = true;
            CountContact(ref player1Contacts, 1, other);
        }

        if (other.CompareTag("Player2"))
        {
            player2Inside = true;
            CountContact(ref player2Contacts, 1, other);
        }

        CheckPortal();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Scene unload also fires trigger exits; those are not the player leaving.
        if (isLoading || !gameObject.scene.isLoaded)
            return;

        if (other.CompareTag("Player1"))
        {
            player1Inside = false;
            CountContact(ref player1Contacts, -1, other);
        }

        if (other.CompareTag("Player2"))
        {
            player2Inside = false;
            CountContact(ref player2Contacts, -1, other);
        }
    }

    // Logs only when the player really enters or leaves, even with several colliders.
    private void CountContact(ref int contacts, int change, Collider2D player)
    {
        bool wasInside = contacts > 0;
        contacts = Mathf.Max(0, contacts + change);

        // Most levels have two EndPortal components on the portal object; only the first logs.
        if (wasInside == contacts > 0 || GetComponent<EndPortal>() != this)
            return;

        AnalyticsManager.Instance?.LogEvent(
            player.transform,
            wasInside ? EventType.PORTAL_EXIT : EventType.PORTAL_ENTER,
            target: this
        );
    }

    private void CheckPortal()
    {
        if (player1Inside && player2Inside && !isLoading)
        {
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                isLoading = true;
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                Debug.Log("All stages completed!");
            }
        }
    }
}