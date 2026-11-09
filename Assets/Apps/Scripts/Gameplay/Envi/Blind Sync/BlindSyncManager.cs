
using UnityEngine;
using CoLab.Telemetry;
using EventType = CoLab.Telemetry.EventType;

public class BlindSyncManager : MonoBehaviour
{
    [Header("Puzzle Settings")]
    [SerializeField] private float syncWindowSeconds = 2f;
    [SerializeField] private BlindSyncSwitch redSwitch;
    [SerializeField] private BlindSyncSwitch blueSwitch;
    [SerializeField] private TargetGroupController targetGroupController;

    [Header("Shared Gate")]
    [SerializeField] private GameObject sharedGate;

    private bool redActivated;
    private bool blueActivated;
    private bool completed;
    private float firstActivationTime;
    private string firstActivationTag;

    public bool IsCompleted => completed;

    private void Start()
    {
        ResetPuzzle();
    }

    private void Update()
    {
        if (completed || (!redActivated && !blueActivated))
            return;

        if (Time.time - firstActivationTime > syncWindowSeconds)
        {
            LogEvent(EventType.SYNC_FAIL, firstActivationTag);
            ResetPuzzle();
        }
    }

    public void RegisterSwitch(string playerTag)
    {
        if (completed)
            return;

        if (playerTag != "Player1" && playerTag != "Player2")
            return;

        if ((playerTag == "Player1" && redActivated) ||
            (playerTag == "Player2" && blueActivated))
            return;

        if (!redActivated && !blueActivated)
        {
            firstActivationTime = Time.time;
            firstActivationTag = playerTag;
        }

        if (playerTag == "Player1")
            redActivated = true;
        else
            blueActivated = true;

        LogEvent(EventType.STEP_ON_BUTTON, playerTag);

        if (redActivated && blueActivated)
        {
            completed = true;

            if (redSwitch != null)
                redSwitch.LockSwitch();

            if (blueSwitch != null)
                blueSwitch.LockSwitch();

            if (targetGroupController != null)
                targetGroupController.FocusOnBothPlayers();

            SetGateOpen(true);
<<<<<<< HEAD

            LogEvent("SYNC_ACTION", "SUCCESS");
=======
            LogEvent(EventType.SYNC_ACTION, playerTag);
>>>>>>> e39a5120dd199c101d37e4cd0dbc4aa422a6facd
        }
    }

    public void ResetPuzzle()
    {
        redActivated = false;
        blueActivated = false;

        if (redSwitch != null)
            redSwitch.ResetSwitch();

        if (blueSwitch != null)
            blueSwitch.ResetSwitch();

        if (!completed)
            SetGateOpen(false);
    }

    private void SetGateOpen(bool open)
    {
        if (sharedGate != null)
            sharedGate.SetActive(!open);
    }

    private void LogEvent(EventType eventType, string playerTag)
    {
        if (AnalyticsManager.Instance == null ||
            !AnalyticsManager.TryGetPlayerID(playerTag, out PlayerID playerID))
            return;

        AnalyticsManager.Instance.LogEvent(playerID, eventType);
    }
}
