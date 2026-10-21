
using UnityEngine;

public class BlindSyncManager : MonoBehaviour
{
    [Header("Puzzle Settings")]
    [SerializeField] private float syncWindowSeconds = 2f;
    [SerializeField] private BlindSyncSwitch redSwitch;
    [SerializeField] private BlindSyncSwitch blueSwitch;

    [Header("Gate Visuals")]
    [SerializeField] private GameObject redGateVisual;
    [SerializeField] private GameObject blueGateVisual;
    [SerializeField] private Collider2D redGateCollider;
    [SerializeField] private Collider2D blueGateCollider;

    private bool redActivated;
    private bool blueActivated;
    private bool completed;
    private float firstActivationTime;

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
            LogEvent("SYNC_FAIL", "TIMEOUT");
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
            firstActivationTime = Time.time;

        if (playerTag == "Player1")
            redActivated = true;
        else
            blueActivated = true;

        LogEvent("STEP_ON_BUTTON", playerTag);

        if (redActivated && blueActivated)
        {
            completed = true;
            SetGateOpen(true);
            LogEvent("SYNC_ACTION", "SUCCESS");
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
        if (redGateVisual != null)
            redGateVisual.SetActive(!open);

        if (blueGateVisual != null)
            blueGateVisual.SetActive(!open);

        if (redGateCollider != null)
            redGateCollider.enabled = !open;

        if (blueGateCollider != null)
            blueGateCollider.enabled = !open;
    }

    private void LogEvent(string eventName, string detail)
    {
        long timestampMs = (long)(Time.time * 1000f);
        Debug.Log($"{eventName} | {detail} | {timestampMs} ms");
    }
}
