using UnityEngine;
using CoLab.Telemetry;
using EventType = CoLab.Telemetry.EventType;

public class PitfallRescueManager : MonoBehaviour
{
    [Header("Players")]
    [SerializeField] private string trappedPlayerTag = "Player2";
    [SerializeField] private string rescuerPlayerTag = "Player1";

    [Header("Rescue Platform")]
    [SerializeField] private RescuePlatform rescuePlatform;

    private Transform trappedPlayer;
    private Collider2D trappedPlayerCollider;

    private float trappedTime;
    private bool rescueStarted;
    private bool rescueCompleted;

    public bool IsPlayerTrapped => rescueStarted && !rescueCompleted;
    public bool IsRescueCompleted => rescueCompleted;
    public Transform TrappedPlayer => trappedPlayer;

    public bool IsTrappedPlayerOnPlatform()
    {
        if (trappedPlayerCollider == null || rescuePlatform == null)
            return false;

        Collider2D platformCollider =
            rescuePlatform.GetComponent<Collider2D>();

        if (platformCollider == null)
        {
            Debug.LogError(
                "PitfallRescueManager: Rescue Platform needs a Collider2D."
            );
            return false;
        }

        return trappedPlayerCollider.IsTouching(platformCollider);
    }

    public void TrapPlayer(GameObject player)
    {
        if (player == null || rescueStarted || rescueCompleted)
            return;

        if (!player.CompareTag(trappedPlayerTag))
            return;

        Collider2D playerCollider =
            player.GetComponent<Collider2D>();

        if (playerCollider == null)
            playerCollider = player.GetComponentInChildren<Collider2D>();

        if (playerCollider == null)
        {
            Debug.LogError(
                "PitfallRescueManager: Player2 needs a Collider2D."
            );
            return;
        }

        trappedPlayer = player.transform;
        trappedPlayerCollider = playerCollider;

        trappedTime = Time.time;
        rescueStarted = true;

        LogEvent(
            trappedPlayerTag,
            EventType.RESCUE_START,
            true,
            0f
        );

        SetTrapped(true);

        Debug.Log(
            "Player2 is trapped. Player1 must activate the rescue lever."
        );
    }

    public void ActivateRescue(GameObject player, Component lever = null)
    {
        if (player == null || !player.CompareTag(rescuerPlayerTag))
            return;

        if (!IsPlayerTrapped || rescueCompleted)
        {
            Debug.Log("Rescue unavailable: Player2 is not trapped.");
            return;
        }

        if (rescuePlatform == null)
        {
            Debug.LogError(
                "PitfallRescueManager: Rescue Platform is not assigned."
            );
            return;
        }

        if (!rescuePlatform.IsReady)
        {
            Debug.LogError(
                "PitfallRescueManager: Rescue Platform is missing its Upper Position."
            );
            return;
        }

        if (!IsTrappedPlayerOnPlatform())
        {
            LogEvent(
                rescuerPlayerTag,
                EventType.RESCUE_ATTEMPT_EARLY,
                false,
                0f,
                lever
            );

            Debug.Log(
                "Rescue unavailable: Player2 is not touching the rescue platform."
            );
            return;
        }

        bool movementStarted = rescuePlatform.StartRescue(trappedPlayer);

        if (!movementStarted)
            return;

        float rescueDuration = Time.time - trappedTime;

        rescueCompleted = true;

        LogEvent(
            rescuerPlayerTag,
            EventType.RESCUE_TIME,
            false,
            rescueDuration,
            lever
        );

        SetTrapped(false);

        Debug.Log("Rescue started successfully!");
    }

    private void SetTrapped(bool trapped)
    {
        if (AnalyticsManager.Instance == null ||
            !AnalyticsManager.TryGetPlayerID(
                trappedPlayerTag,
                out PlayerID trappedPlayerID))
            return;

        AnalyticsManager.Instance.SetPlayerTrapped(
            trappedPlayerID,
            trapped
        );
    }

    private void LogEvent(
        string playerTag,
        EventType eventType,
        bool isTrapped,
        float durationSeconds,
        Component target = null)
    {
        if (AnalyticsManager.Instance == null ||
            !AnalyticsManager.TryGetPlayerID(
                playerTag,
                out PlayerID playerID))
            return;

        AnalyticsManager.Instance.LogEvent(
            playerID,
            eventType,
            isTrapped,
            durationSeconds,
            target
        );
    }
}
