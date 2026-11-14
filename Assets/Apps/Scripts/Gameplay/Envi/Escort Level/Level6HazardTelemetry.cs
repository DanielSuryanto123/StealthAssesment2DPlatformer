
using UnityEngine;
using CoLab.Telemetry;

[System.Obsolete("Level6DeadzoneTrigger logs HAZARD_ZONE_ENTERED; kept only so scenes/prefabs referencing it still load")]
public class Level6HazardTelemetry : MonoBehaviour
{
    [SerializeField] private bool isTrapped = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player1") &&
            !other.CompareTag("Player2"))
        {
            return;
        }

        AnalyticsManager.Instance?.LogHazardEntered(
            other.transform,
            isTrapped
        );
    }
}
