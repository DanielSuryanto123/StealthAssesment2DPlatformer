using System;
using System.Collections.Generic;
using UnityEngine;

public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance { get; private set; }

    [Serializable]
    public class AnalyticsEvent
    {
        public string eventName;
        public string playerId;
        public string details;
        public long timestampMs;
        public long endTimestampMs;
        public long durationMs;
    }

    [Serializable]
    private class EventList
    {
        public List<AnalyticsEvent> events;

        public EventList(List<AnalyticsEvent> events)
        {
            this.events = events;
        }
    }

    private class ActiveEvent
    {
        public string eventName;
        public string playerId;
        public string details;
        public long startTimestampMs;
    }

    public List<AnalyticsEvent> Events { get; private set; }
        = new List<AnalyticsEvent>();

    private readonly Dictionary<string, ActiveEvent> activeEvents
        = new Dictionary<string, ActiveEvent>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private long GetTimestampMs()
    {
        return (long)(Time.timeAsDouble * 1000.0);
    }

    public void LogInstantEvent(
        string eventName,
        string playerId,
        string details = "")
    {
        long timestamp = GetTimestampMs();

        Events.Add(new AnalyticsEvent
        {
            eventName = eventName,
            playerId = playerId,
            details = details,
            timestampMs = timestamp,
            endTimestampMs = timestamp,
            durationMs = 0
        });

        Debug.Log(
            $"[Analytics] {eventName} | Player: {playerId} | " +
            $"Timestamp: {timestamp} ms | Duration: 0 ms | {details}"
        );
    }

    public void BeginTimedEvent(
        string eventKey,
        string eventName,
        string playerId,
        string details = "")
    {
        if (activeEvents.ContainsKey(eventKey))
            return;

        activeEvents.Add(eventKey, new ActiveEvent
        {
            eventName = eventName,
            playerId = playerId,
            details = details,
            startTimestampMs = GetTimestampMs()
        });
    }

    public void EndTimedEvent(string eventKey)
    {
        if (!activeEvents.TryGetValue(eventKey, out ActiveEvent active))
            return;

        long endTimestamp = GetTimestampMs();
        long duration = Math.Max(
            0,
            endTimestamp - active.startTimestampMs
        );

        Events.Add(new AnalyticsEvent
        {
            eventName = active.eventName,
            playerId = active.playerId,
            details = active.details,
            timestampMs = active.startTimestampMs,
            endTimestampMs = endTimestamp,
            durationMs = duration
        });

        Debug.Log(
            $"[Analytics] {active.eventName} | Player: {active.playerId} | " +
            $"Timestamp: {active.startTimestampMs} ms | " +
            $"Duration: {duration} ms | {active.details}"
        );

        activeEvents.Remove(eventKey);
    }

    public string ExportJson()
    {
        return JsonUtility.ToJson(new EventList(Events), true);
    }
}