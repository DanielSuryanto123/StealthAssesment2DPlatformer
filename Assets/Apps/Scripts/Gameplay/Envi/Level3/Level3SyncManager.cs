
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

    public void RegisterSwitch(string playerTag)
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

        CheckSynchronization();
    }

    private void CheckSynchronization()
    {
        if (!redSwitchActivated || !blueSwitchActivated)
            return;

        puzzleCompleted = true;

        if (sharedDoor != null)
            sharedDoor.OpenDoor();

        // Record successful synchronization for both players.
        if (AnalyticsManager.Instance != null)
        {
            AnalyticsManager.Instance.LogEvent(
                PlayerID.PLAYER_1,
                TelemetryEventType.SYNC_ACTION
            );

            AnalyticsManager.Instance.LogEvent(
                PlayerID.PLAYER_2,
                TelemetryEventType.SYNC_ACTION
            );
        }

        Debug.Log("LEVEL 3 SYNC SUCCESS! Shared door opened.");
    }
}
