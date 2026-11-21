using System.Collections.Generic;
using UnityEngine;
using CoLab.Telemetry;
using TelemetryEventType = CoLab.Telemetry.EventType;

[RequireComponent(typeof(Collider2D))]
public class Level3SyncSwitch : MonoBehaviour
{
    [Header("Switch Settings")]
    [SerializeField] private string requiredPlayerTag = "Player1";

    [Header("Visual Settings")]
    [SerializeField] private SpriteRenderer switchRenderer;
    [SerializeField] private Sprite offSprite;
    [SerializeField] private Sprite onSprite;

    [Header("Manager")]
    [SerializeField] private Level3SyncManager syncManager;

    private readonly Dictionary<string, int> playerContactCount =
        new Dictionary<string, int>();

    private InputHandler nearbyInput;
    private bool isActivated;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;

        if (switchRenderer == null)
            switchRenderer = GetComponent<SpriteRenderer>();

        UpdateVisual();
    }

    private void Update()
    {
        if (nearbyInput == null || isActivated)
            return;

        if (nearbyInput.InteractPressed)
        {
            nearbyInput.ResetInteract();
            ActivateSwitch();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Transform playerRoot = other.transform.root;
        string playerTag = playerRoot.tag;

        if (playerTag != "Player1" && playerTag != "Player2")
            return;

        if (playerTag != requiredPlayerTag)
            return;

        int count = playerContactCount.ContainsKey(playerTag)
            ? playerContactCount[playerTag]
            : 0;

        playerContactCount[playerTag] = count + 1;

        if (count > 0)
            return;

        InputHandler input = playerRoot.GetComponent<InputHandler>();

        if (input == null)
            input = playerRoot.GetComponentInChildren<InputHandler>();

        nearbyInput = input;

        Debug.Log(playerTag + " is near " + gameObject.name);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Transform playerRoot = other.transform.root;
        string playerTag = playerRoot.tag;

        if (playerTag != requiredPlayerTag)
            return;

        if (!playerContactCount.ContainsKey(playerTag))
            return;

        playerContactCount[playerTag]--;

        if (playerContactCount[playerTag] > 0)
            return;

        playerContactCount.Remove(playerTag);

        nearbyInput = null;

        // Reset the switch when the player leaves.
        isActivated = false;
        UpdateVisual();

        Debug.Log(playerTag + " left " + gameObject.name);
    }

    private void ActivateSwitch()
    {
        if (syncManager == null)
        {
            Debug.LogWarning("Level3SyncManager is not assigned!");
            return;
        }

        isActivated = true;
        UpdateVisual();

        Debug.Log(requiredPlayerTag + " activated " + gameObject.name);

        RecordButtonEvent(
            TelemetryEventType.STEP_ON_BUTTON,
            requiredPlayerTag
        );

        syncManager.RegisterSwitch(requiredPlayerTag);
    }

    private void UpdateVisual()
    {
        if (switchRenderer == null)
            return;

        switchRenderer.sprite = isActivated
            ? onSprite
            : offSprite;
    }

    private void RecordButtonEvent(
        TelemetryEventType eventType,
        string playerTag)
    {
        if (AnalyticsManager.Instance == null)
            return;

        if (AnalyticsManager.TryGetPlayerID(
            playerTag, out PlayerID playerId))
        {
            AnalyticsManager.Instance.LogEvent(
                playerId,
                eventType
            );
        }
    }
}
