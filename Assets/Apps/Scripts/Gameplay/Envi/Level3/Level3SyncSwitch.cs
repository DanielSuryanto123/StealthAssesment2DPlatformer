
using System.Collections.Generic;
using UnityEngine;
using CoLab.Telemetry;
using TelemetryEventType = CoLab.Telemetry.EventType;

[RequireComponent(typeof(Collider2D))]
public class Level3SyncSwitch : MonoBehaviour
{
    [Header("Switch Settings")]
    [SerializeField] private string requiredPlayerTag = "Player1";

    [Header("Manager")]
    [SerializeField] private Level3SyncManager syncManager;

    private readonly Dictionary<string, int> playerContactCount =
        new Dictionary<string, int>();

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Transform playerRoot = other.transform.root;
        string playerTag = playerRoot.tag;

        if (playerTag != "Player1" && playerTag != "Player2")
            return;

        if (playerTag != requiredPlayerTag)
            return;

        int count = playerContactCount.ContainsKey(playerTag)
            ? playerContactCount[playerTag]
            : 0;

        playerContactCount[playerTag] = count + 1;

        // Only record the first collider entering.
        if (count > 0)
            return;

        Debug.Log(playerTag + " stepped onto " + gameObject.name);

        RecordButtonEvent(
            TelemetryEventType.STEP_ON_BUTTON,
            playerTag
        );

        if (syncManager != null)
            syncManager.RegisterSwitch(playerTag);
        else
            Debug.LogWarning("Level3SyncManager is not assigned!");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Transform playerRoot = other.transform.root;
        string playerTag = playerRoot.tag;

        if (playerTag != requiredPlayerTag)
            return;

        if (!playerContactCount.ContainsKey(playerTag))
            return;

        playerContactCount[playerTag]--;

        if (playerContactCount[playerTag] > 0)
            return;

        playerContactCount.Remove(playerTag);

        Debug.Log(playerTag + " stepped off " + gameObject.name);

        RecordButtonEvent(
            TelemetryEventType.STEP_OFF_BUTTON,
            playerTag
        );
    }

    private void RecordButtonEvent(
        TelemetryEventType eventType,
        string playerTag)
    {
        if (AnalyticsManager.Instance == null)
            return;

        if (AnalyticsManager.TryGetPlayerID(
            playerTag, out PlayerID playerId))
        {
            AnalyticsManager.Instance.LogEvent(
                playerId,
                eventType
            );
        }
    }
}
