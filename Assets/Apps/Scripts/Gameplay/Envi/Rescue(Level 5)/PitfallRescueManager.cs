
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
            return false;

        return trappedPlayerCollider.IsTouching(platformCollider);
    }

    public void TrapPlayer(GameObject player)
    {
        if (player == null || rescueStarted || rescueCompleted)
            return;

        if (!player.CompareTag(trappedPlayerTag))
            return;

        trappedPlayer = player.transform;
        trappedPlayerCollider = player.GetComponent<Collider2D>();

        if (trappedPlayerCollider == null)
        {
            Debug.LogError("PitfallRescueManager: Player2 needs a Collider2D.");
            trappedPlayer = null;
            return;
        }

        trappedTime = Time.time;
        rescueStarted = true;

        LogEvent(trappedPlayerTag, EventType.RESCUE_START, true, 0f);
        SetTrapped(true);
        Debug.Log("Player 2 is trapped. Player 1 must activate the rescue lever.");
    }

    public void ActivateRescue(GameObject player)
    {
        if (player == null || !player.CompareTag(rescuerPlayerTag))
            return;

        if (!IsPlayerTrapped || rescueCompleted)
            return;

        if (!IsTrappedPlayerOnPlatform())
        {
            Debug.Log("Rescue unavailable: Player 2 is not on the rescue platform.");
            return;
        }

        if (rescuePlatform == null)
        {
            Debug.LogError("PitfallRescueManager: Rescue Platform is not assigned.");
            return;
        }

        float rescueDuration = Time.time - trappedTime;
        rescueCompleted = true;

        LogEvent(rescuerPlayerTag, EventType.RESCUE_TIME, false, rescueDuration);
        SetTrapped(false);
        rescuePlatform.StartRescue(trappedPlayer);
    }

    private void SetTrapped(bool trapped)
    {
        if (AnalyticsManager.Instance == null ||
            !AnalyticsManager.TryGetPlayerID(trappedPlayerTag, out PlayerID trappedPlayerID))
            return;

        AnalyticsManager.Instance.SetPlayerTrapped(trappedPlayerID, trapped);
    }

    private void LogEvent(
        string playerTag,
        EventType eventType,
        bool isTrapped,
        float durationSeconds)
    {
        if (AnalyticsManager.Instance == null ||
            !AnalyticsManager.TryGetPlayerID(playerTag, out PlayerID playerID))
            return;

        AnalyticsManager.Instance.LogEvent(
            playerID, eventType, isTrapped, durationSeconds
        );
    }
}
