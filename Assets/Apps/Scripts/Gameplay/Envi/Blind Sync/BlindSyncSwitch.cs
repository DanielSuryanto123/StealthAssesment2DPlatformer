
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
    [SerializeField] private Sprite offSprite;
    [SerializeField] private Sprite onSprite;

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
            input.ResetInteract();

            activated = true;
            UpdateVisual();

            syncManager.RegisterSwitch(requiredPlayerTag);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
{
    Transform playerRoot = other.transform.root;

    string playerTag = playerRoot.tag;

    if (playerTag != "Player1" && playerTag != "Player2")
        return;

    InputHandler input = playerRoot.GetComponent<InputHandler>();

    if (input == null)
        input = other.GetComponentInParent<InputHandler>();

    if (input == null)
    {
        Debug.LogWarning(playerRoot.name + " has no InputHandler!");
        return;
    }

    playersNearby[playerTag] = input;

    Debug.Log(playerRoot.name + " entered switch. Tag: " + playerTag);
}

private void OnTriggerExit2D(Collider2D other)
{
    Transform playerRoot = other.transform.root;
    string playerTag = playerRoot.tag;

    if (playerTag == "Player1" || playerTag == "Player2")
    {
        playersNearby.Remove(playerTag);
        Debug.Log(playerRoot.name + " exited switch.");
    }
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

    public void LockSwitch()
    {
        activated = true;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (switchRenderer == null)
            return;

        switchRenderer.sprite = activated ? onSprite : offSprite;
    }
}
