
using System;
using System.Collections.Generic;
using UnityEngine;

public enum EventType
{
    MOVE_START,
    MOVE_STOP,
    JUMP_SUCCESS,
    JUMP_FAIL,
    LEVEL_RESTART,
    PULL_LEVER,
    RELEASE_LEVER,
    INVALID_INTERACT,
    STEP_ON_BUTTON,
    STEP_OFF_BUTTON,
    PING_HELP,
    PING_HERE,
    SYNC_ACTION,
    WAIT_PARTNER,
    RESCUE_TIME,
    HAZARD_ZONE_ENTERED
}

public enum PlayerID
{
    Player1,
    Player2
}

[Serializable]
public class Position
{
    public float x;
    public float y;

    public Position(float x, float y)
    {
        this.x = x;
        this.y = y;
    }
}

[Serializable]
public class TelemetryDataLog
{
    public float timestamp;
    public string event_type;
    public string player_id;
    public Position player_pos;
    public Position partner_pos;
    public bool isTrapped;
    public float duration;
}

[Serializable]
public class TelemetryDataContainer
{
    public List<TelemetryDataLog> telemetryDataLogs =
        new List<TelemetryDataLog>();
}

public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance { get; private set; }

    [Header("Players")]
    [SerializeField] private Transform player1;
    [SerializeField] private Transform player2;

    [Header("Telemetry")]
    [SerializeField] private bool logToConsole = true;

    private TelemetryDataContainer telemetry =
        new TelemetryDataContainer();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetPlayers(Transform p1, Transform p2)
    {
        player1 = p1;
        player2 = p2;
    }

    public void LogEvent(
        PlayerID playerID,
        EventType eventType,
        bool isTrapped = false,
        float duration = 0f)
    {
        Transform actor = GetPlayer(playerID);
        Transform partner = GetPlayer(
            playerID == PlayerID.Player1
                ? PlayerID.Player2
                : PlayerID.Player1
        );

        if (actor == null)
        {
            Debug.LogWarning(
                $"AnalyticsManager: {playerID} is not assigned."
            );
            return;
        }

        TelemetryDataLog log = new TelemetryDataLog
        {
            timestamp = Time.time,
            event_type = eventType.ToString(),
            player_id = playerID.ToString(),
            player_pos = new Position(
                actor.position.x,
                actor.position.y
            ),
            partner_pos = partner != null
                ? new Position(partner.position.x, partner.position.y)
                : new Position(0f, 0f),
            isTrapped = isTrapped,
            duration = UsesDuration(eventType)
                ? Mathf.Max(0f, duration)
                : 0f
        };

        telemetry.telemetryDataLogs.Add(log);

        if (logToConsole)
        {
            Debug.Log(JsonUtility.ToJson(log));
        }
    }

    public void LogLeverPull(Transform actor)
    {
        if (TryGetPlayerID(actor, out PlayerID playerID))
        {
            LogEvent(playerID, EventType.PULL_LEVER);
        }
    }

    public void LogHazardEntered(Transform actor, bool isTrapped)
    {
        if (TryGetPlayerID(actor, out PlayerID playerID))
        {
            LogEvent(
                playerID,
                EventType.HAZARD_ZONE_ENTERED,
                isTrapped
            );
        }
    }

    public string GetAllLogsAsJson()
    {
        return JsonUtility.ToJson(telemetry, true);
    }

    public int GetLogCount()
    {
        return telemetry.telemetryDataLogs.Count;
    }

    private Transform GetPlayer(PlayerID playerID)
    {
        return playerID == PlayerID.Player1
            ? player1
            : player2;
    }

    private bool TryGetPlayerID(
        Transform actor,
        out PlayerID playerID)
    {
        if (actor != null && actor.CompareTag("Player1"))
        {
            playerID = PlayerID.Player1;
            return true;
        }

        if (actor != null && actor.CompareTag("Player2"))
        {
            playerID = PlayerID.Player2;
            return true;
        }

        playerID = PlayerID.Player1;
        return false;
    }

    private bool UsesDuration(EventType eventType)
    {
        return eventType == EventType.WAIT_PARTNER
            || eventType == EventType.RESCUE_TIME
            || eventType == EventType.LEVEL_RESTART;
    }
}
