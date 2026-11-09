
using System.Collections.Generic;
using CoLab.Telemetry;
using UnityEngine;
using EventType = CoLab.Telemetry.EventType;

public class BlindSyncSwitch : MonoBehaviour
{
    [Header("Switch Settings")]
    [SerializeField] private string requiredPlayerTag = "Player1";
    [SerializeField] private BlindSyncManager syncManager;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer switchRenderer;
    [SerializeField] private Color inactiveColor = Color.gray;
    [SerializeField] private Color activeColor = Color.green;

    private readonly Dictionary<string, InputHandler> playersNearby = new();
    private bool activated;

    private const float InvalidInteractDebounceSeconds = 0.25f;
    private readonly Dictionary<string, float> lastInvalidInteractTime = new();

    private void Start()
    {
        UpdateVisual();
    }

    private void Update()
    {
        if (syncManager == null || syncManager.IsCompleted)
            return;

        string wrongTag = requiredPlayerTag == "Player1" ? "Player2" : "Player1";

        if (playersNearby.TryGetValue(wrongTag, out InputHandler wrongInput) &&
            wrongInput != null && wrongInput.InteractPressed)
        {
            LogInvalidInteract(wrongTag, wrongInput.transform);
        }

        if (activated)
            return;

        if (playersNearby.TryGetValue(requiredPlayerTag, out InputHandler input) &&
            input != null && input.InteractPressed)
        {
            activated = true;
            UpdateVisual();
            syncManager.RegisterSwitch(requiredPlayerTag);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        InputHandler input = other.GetComponent<InputHandler>();

        if (input == null)
            input = other.GetComponentInParent<InputHandler>();

        if (input == null)
            return;

        if (other.CompareTag("Player1"))
            playersNearby["Player1"] = input;
        else if (other.CompareTag("Player2"))
            playersNearby["Player2"] = input;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player1"))
            playersNearby.Remove("Player1");
        else if (other.CompareTag("Player2"))
            playersNearby.Remove("Player2");
    }

    private void LogInvalidInteract(string playerTag, Transform actor)
    {
        if (lastInvalidInteractTime.TryGetValue(playerTag, out float last) &&
            Time.time - last < InvalidInteractDebounceSeconds)
            return;

        lastInvalidInteractTime[playerTag] = Time.time;
        AnalyticsManager.Instance?.LogEvent(actor, EventType.INVALID_INTERACT);
    }

    public void ResetSwitch()
    {
        activated = false;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (switchRenderer != null)
            switchRenderer.color = activated ? activeColor : inactiveColor;
    }
}
