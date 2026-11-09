
using CoLab.Telemetry;
using UnityEngine;
using EventType = CoLab.Telemetry.EventType;

public class CoopDoorController : MonoBehaviour
{
    [Header("Door")]
    [SerializeField] private GameObject door;

    [Header("Co-op Settings")]
    [SerializeField] private float activationWindowSeconds = 2f;

    private LeverController player1Lever;
    private LeverController player2Lever;

    private bool player1Activated;
    private bool player2Activated;
    private bool puzzleCompleted;
    private float firstActivationTime;
    private Transform firstActor;

    private void Start()
    {
        ResetPuzzle();
    }

    private void Update()
    {
        if (puzzleCompleted)
            return;

        if ((player1Activated || player2Activated) &&
            Time.time - firstActivationTime > activationWindowSeconds)
        {
            LogSync(firstActor, EventType.SYNC_FAIL);
            ResetPuzzle();
            Debug.Log("Co-op puzzle timed out. Levers reset.");
        }
    }

    public void RegisterLever(string playerTag, LeverController lever, Transform actor = null)
    {
        if (puzzleCompleted || lever == null)
            return;

        if (playerTag != lever.RequiredPlayerTag)
            return;

        if (player1Activated || player2Activated)
        {
            if (Time.time - firstActivationTime > activationWindowSeconds)
            {
                LogSync(firstActor, EventType.SYNC_FAIL);
                ResetPuzzle();
            }
        }

        if (!player1Activated && !player2Activated)
        {
            firstActivationTime = Time.time;
            firstActor = actor;
        }

        if (playerTag == "Player1")
        {
            player1Activated = true;
            player1Lever = lever;
        }
        else if (playerTag == "Player2")
        {
            player2Activated = true;
            player2Lever = lever;
        }
        else
        {
            return;
        }

        if (player1Activated && player2Activated)
        {
            puzzleCompleted = true;
            LogSync(actor, EventType.SYNC_ACTION);

            if (door != null)
                door.SetActive(false);

            Debug.Log("Co-op puzzle completed! Door opened.");
        }
    }

    private void LogSync(Transform actor, EventType type)
    {
        // Skip the Door prefab asset, and Level 4 where BlindSyncManager owns sync telemetry.
        if (actor == null || !gameObject.scene.IsValid())
            return;

        if ((player1Lever != null && player1Lever.GetComponent<BlindSyncSwitch>() != null) ||
            (player2Lever != null && player2Lever.GetComponent<BlindSyncSwitch>() != null))
            return;

        AnalyticsManager.Instance?.LogEvent(actor, type);
    }

    private void ResetPuzzle()
    {
        firstActor = null;
        player1Activated = false;
        player2Activated = false;

        LeverController first = player1Lever;
        LeverController second = player2Lever;

        player1Lever = null;
        player2Lever = null;

        if (first != null)
            first.ResetLever();

        if (second != null && second != first)
            second.ResetLever();

        if (door != null)
            door.SetActive(true);
    }
}
