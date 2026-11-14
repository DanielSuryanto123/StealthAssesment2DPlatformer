
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Level3TelemetryEvent : UnityEvent<string, string> { }

public class Level3SyncManager : MonoBehaviour
{
    [Header("Door")]
    [SerializeField] private SinglePlayerDoorController sharedDoor;

    [Header("Switch States")]
    [SerializeField] private bool redSwitchActivated;
    [SerializeField] private bool blueSwitchActivated;

    [Header("Telemetry")]
    public Level3TelemetryEvent onTelemetryEvent;

    private bool puzzleCompleted;

    public void RegisterSwitch(string playerTag)
    {
        if (puzzleCompleted)
            return;

        if (playerTag == "Player1")
            redSwitchActivated = true;
        else if (playerTag == "Player2")
            blueSwitchActivated = true;
        else
            return;

        Debug.Log(playerTag + " activated their sync switch!");

        CheckSynchronization();
    }

    public void RecordButtonEvent(string eventType, string playerTag)
    {
        onTelemetryEvent?.Invoke(eventType, playerTag);
    }

    private void CheckSynchronization()
    {
        if (redSwitchActivated && blueSwitchActivated)
        {
            puzzleCompleted = true;

            if (sharedDoor != null)
                sharedDoor.OpenDoor();

            RecordButtonEvent("SYNC_ACTION", "BOTH");

            Debug.Log("LEVEL 3 SYNC SUCCESS! Door opened.");
        }
    }
}
