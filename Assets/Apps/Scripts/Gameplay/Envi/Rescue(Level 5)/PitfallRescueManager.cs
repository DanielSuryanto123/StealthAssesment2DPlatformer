
using UnityEngine;

public class PitfallRescueManager : MonoBehaviour
{
    [Header("Players")]
    [SerializeField] private string trappedPlayerTag = "Player2";
    [SerializeField] private string rescuerPlayerTag = "Player1";

    [Header("Rescue Platform")]
    [SerializeField] private RescuePlatform rescuePlatform;

    private Transform trappedPlayer;
    private float trappedTime;
    private bool rescueStarted;
    private bool rescueCompleted;

    public bool IsPlayerTrapped => rescueStarted && !rescueCompleted;
    public bool IsRescueCompleted => rescueCompleted;
    public Transform TrappedPlayer => trappedPlayer;

    public void TrapPlayer(GameObject player)
    {
        if (player == null || rescueStarted || rescueCompleted)
            return;

        if (!player.CompareTag(trappedPlayerTag))
            return;

        trappedPlayer = player.transform;
        trappedTime = Time.time;
        rescueStarted = true;

        LogEvent("RESCUE_START", 0f);

        Debug.Log("Player 2 is trapped. Player 1 must activate the rescue lever.");
    }

    public void ActivateRescue(GameObject player)
    {
        if (player == null || !player.CompareTag(rescuerPlayerTag))
            return;

        if (!IsPlayerTrapped || rescueCompleted)
            return;

        if (rescuePlatform == null)
        {
            Debug.LogError("PitfallRescueManager: Rescue Platform is not assigned.");
            return;
        }

        float rescueDuration = Time.time - trappedTime;
        rescueCompleted = true;

        LogEvent("RESCUE_TIME", rescueDuration);

        rescuePlatform.StartRescue(trappedPlayer);
    }

    private void LogEvent(string eventName, float durationSeconds)
    {
        long timestampMs = (long)(Time.time * 1000f);
        long durationMs = (long)(durationSeconds * 1000f);

        Debug.Log(
            $"{eventName} | Duration: {durationMs} ms | Timestamp: {timestampMs} ms"
        );
    }
}
