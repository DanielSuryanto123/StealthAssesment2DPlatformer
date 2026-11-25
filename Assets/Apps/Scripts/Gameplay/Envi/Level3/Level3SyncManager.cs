
using UnityEngine;
using CoLab.Telemetry;
using TelemetryEventType = CoLab.Telemetry.EventType;

public class Level3SyncManager : MonoBehaviour
{
    [Header("Shared Door")]
    [SerializeField] private SinglePlayerDoorController sharedDoor;

    private bool redSwitchActivated;
    private bool blueSwitchActivated;
    private bool puzzleCompleted;
    private float firstActivationTime;

    public bool IsCompleted => puzzleCompleted;

    public void RegisterSwitch(string playerTag, Component source = null)
    {
        if (puzzleCompleted)
            return;

        if (playerTag == "Player1")
        {
            if (redSwitchActivated)
                return;

            redSwitchActivated = true;
            Debug.Log("Player1 activated the red switch!");
        }
        else if (playerTag == "Player2")
        {
            if (blueSwitchActivated)
                return;

            blueSwitchActivated = true;
            Debug.Log("Player2 activated the blue switch!");
        }
        else
        {
            return;
        }

        CheckSynchronization(playerTag, source);
    }

    /// <summary>The player left their switch before the partner pressed: the attempt failed.</summary>
    public void UnregisterSwitch(string playerTag, Component source = null)
    {
        if (puzzleCompleted)
            return;

        if (playerTag == "Player1" && redSwitchActivated)
            redSwitchActivated = false;
        else if (playerTag == "Player2" && blueSwitchActivated)
            blueSwitchActivated = false;
        else
            return;

        LogEvent(TelemetryEventType.STEP_OFF_BUTTON, playerTag, source);
        LogEvent(TelemetryEventType.SYNC_FAIL, playerTag, source);
    }

    private void CheckSynchronization(string followerTag, Component source)
    {
        if (!redSwitchActivated || !blueSwitchActivated)
        {
            firstActivationTime = Time.time;
            return;
        }

        puzzleCompleted = true;

        if (sharedDoor != null)
            sharedDoor.OpenDoor();

        // Only the follower gets SYNC_ACTION; duration is the gap between the two presses.
        LogEvent(
            TelemetryEventType.SYNC_ACTION,
            followerTag,
            source,
            Time.time - firstActivationTime
        );

        Debug.Log("LEVEL 3 SYNC SUCCESS! Shared door opened.");
    }

    private static void LogEvent(
        TelemetryEventType eventType,
        string playerTag,
        Component source,
        float duration = 0f)
    {
        if (AnalyticsManager.Instance == null ||
            !AnalyticsManager.TryGetPlayerID(playerTag, out PlayerID playerId))
            return;

        AnalyticsManager.Instance.LogEvent(playerId, eventType, false, duration, source);
    }
}
