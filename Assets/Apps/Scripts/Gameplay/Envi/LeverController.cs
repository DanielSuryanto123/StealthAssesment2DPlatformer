
using System.Collections.Generic;
using CoLab.Telemetry;
using UnityEngine;
using EventType = CoLab.Telemetry.EventType;

[RequireComponent(typeof(Collider2D))]
public class LeverController : MonoBehaviour
{
    public enum ActivationMode
    {
        SingleUse,
        PermanentButton
    }

    [Header("Player Access")]
    [SerializeField] private string requiredPlayerTag = "Player1";

    [Header("Activation Mode")]
    [SerializeField] private ActivationMode activationMode = ActivationMode.SingleUse;

    [Header("Co-op Puzzle")]
    [SerializeField] private CoopDoorController coopPuzzle;

    [Header("Laser Hazards")]
    [SerializeField] private List<LaserHazard> linkedHazards = new List<LaserHazard>();
    [SerializeField] private bool deactivateHazardsOnUse = true;

    [Header("Interaction")]
    [SerializeField] private bool singleUse = true;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer leverVisual;
    [SerializeField] private Sprite leverOnSprite;
    [SerializeField] private Sprite leverOffSprite;
    [SerializeField] private GameObject wrongColorFeedback;
    [SerializeField] private GameObject activationVfx;

    private readonly Dictionary<string, InputHandler> playersNearby =
        new Dictionary<string, InputHandler>();

    private bool activated;
    private bool telemetrySuppressed;
    private Transform lastActor;

    private const float InvalidInteractDebounceSeconds = 0.25f;
    private readonly Dictionary<string, float> lastInvalidInteractTime =
        new Dictionary<string, float>();

    public event System.Action<Transform> Activated;

    public string RequiredPlayerTag => requiredPlayerTag;
    public bool IsActivated => activated;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        // Level 4 levers are owned by BlindSyncManager, which logs its own events.
        telemetrySuppressed = GetComponent<BlindSyncSwitch>() != null;
        UpdateVisual(false);
    }

    private void Update()
    {
        bool lockedOut = activated && (activationMode == ActivationMode.PermanentButton || singleUse);

        foreach (var entry in playersNearby)
        {
            InputHandler input = entry.Value;

            if (input == null || !input.InteractPressed)
                continue;

            if (entry.Key != requiredPlayerTag)
            {
                LogInvalidInteract(entry.Key, input.transform);

                if (lockedOut)
                    continue;

                if (wrongColorFeedback != null)
                {
                    Instantiate(
                        wrongColorFeedback,
                        transform.position,
                        Quaternion.identity
                    );
                }

                continue;
            }

            if (lockedOut)
                continue;

            ActivateLever(input.transform);
            break;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        string playerTag = other.tag;

        if (playerTag != "Player1" && playerTag != "Player2")
            return;

        InputHandler input = other.GetComponent<InputHandler>();

        if (input == null)
            input = other.GetComponentInParent<InputHandler>();

        if (input != null)
            playersNearby[playerTag] = input;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        string playerTag = other.tag;

        if (!playersNearby.TryGetValue(playerTag, out InputHandler input))
            return;

        InputHandler exitingInput = other.GetComponent<InputHandler>();

        if (exitingInput == null)
            exitingInput = other.GetComponentInParent<InputHandler>();

        if (input == exitingInput)
            playersNearby.Remove(playerTag);
    }

    private void LogInvalidInteract(string playerTag, Transform actor)
    {
        if (telemetrySuppressed)
            return;

        if (lastInvalidInteractTime.TryGetValue(playerTag, out float last) &&
            Time.time - last < InvalidInteractDebounceSeconds)
            return;

        lastInvalidInteractTime[playerTag] = Time.time;
        AnalyticsManager.Instance?.LogEvent(actor, EventType.INVALID_INTERACT);
    }

    public void ActivateLever(Transform actor)
    {
        if (activated)
            return;

        activated = true;
        lastActor = actor;
        UpdateVisual(true);

        foreach (LaserHazard hazard in linkedHazards)
        {
            if (hazard != null)
                hazard.SetActive(!deactivateHazardsOnUse);
        }

        if (activationVfx != null)
            Instantiate(activationVfx, transform.position, Quaternion.identity);

        if (coopPuzzle != null)
            coopPuzzle.RegisterLever(requiredPlayerTag, this, actor);

        if (!telemetrySuppressed)
            AnalyticsManager.Instance?.LogEvent(actor, EventType.PULL_LEVER);

        Activated?.Invoke(actor);
    }

    public void ResetLever()
    {
        // RELEASE_LEVER: the lever the player pulled springs back after the co-op window timed out (never on success).
        if (activated && !telemetrySuppressed && lastActor != null && gameObject.scene.IsValid())
            AnalyticsManager.Instance?.LogEvent(lastActor, EventType.RELEASE_LEVER);

        activated = false;
        lastActor = null;
        UpdateVisual(false);

        foreach (LaserHazard hazard in linkedHazards)
        {
            if (hazard != null)
                hazard.ResetToStartState();
        }
    }

    private void UpdateVisual(bool isOn)
    {
        if (leverVisual == null)
            return;

        Sprite targetSprite = isOn ? leverOnSprite : leverOffSprite;

        if (targetSprite != null)
            leverVisual.sprite = targetSprite;
    }
}
