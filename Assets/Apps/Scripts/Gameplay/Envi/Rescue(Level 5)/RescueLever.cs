
using CoLab.Telemetry;
using UnityEngine;
using UnityEngine.InputSystem;
using EventType = CoLab.Telemetry.EventType;

public class RescueLever : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PitfallRescueManager rescueManager;
    [SerializeField] private SpriteRenderer leverVisual;
    [SerializeField] private Sprite leverOffSprite;
    [SerializeField] private Sprite leverOnSprite;

    [Header("Interaction")]
    [SerializeField] private string requiredPlayerTag = "Player1";

    private InputHandler nearbyInput;
    private GameObject nearbyPlayer;
    private GameObject wrongPlayer;
    private InputHandler wrongInput;
    private bool activated;

    private const float InvalidInteractDebounceSeconds = 0.25f;
    private readonly System.Collections.Generic.Dictionary<string, float> lastInvalidInteractTime =
        new System.Collections.Generic.Dictionary<string, float>();

    private void Start()
    {
        SetVisual(false);
    }

    private void Update()
    {
        if (wrongInput != null && wrongPlayer != null && wrongInput.InteractPressed)
            LogInvalidInteract(wrongPlayer.tag, wrongPlayer.transform);

        if (activated || nearbyInput == null || nearbyPlayer == null)
            return;

        if (nearbyInput.InteractPressed)
        {
            if (rescueManager == null)
            {
                Debug.LogError("RescueLever: Rescue Manager is missing.");
                return;
            }

            rescueManager.ActivateRescue(nearbyPlayer);

            if (rescueManager.IsRescueCompleted)
            {
                activated = true;
                SetVisual(true);
                AnalyticsManager.Instance?.LogEvent(nearbyPlayer.transform, EventType.PULL_LEVER);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerInput playerInput = other.GetComponentInParent<PlayerInput>();

        if (playerInput == null)
            return;

        GameObject player = playerInput.gameObject;

        if (!player.CompareTag(requiredPlayerTag))
        {
            if (player.CompareTag("Player1") || player.CompareTag("Player2"))
            {
                wrongPlayer = player;
                wrongInput = player.GetComponent<InputHandler>();
            }

            return;
        }

        nearbyPlayer = player;
        nearbyInput = player.GetComponent<InputHandler>();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerInput playerInput = other.GetComponentInParent<PlayerInput>();

        if (playerInput == null)
            return;

        if (wrongPlayer == playerInput.gameObject)
        {
            wrongPlayer = null;
            wrongInput = null;
            return;
        }

        if (nearbyPlayer != playerInput.gameObject)
            return;

        nearbyPlayer = null;
        nearbyInput = null;
    }

    private void LogInvalidInteract(string playerTag, Transform actor)
    {
        if (lastInvalidInteractTime.TryGetValue(playerTag, out float last) &&
            Time.time - last < InvalidInteractDebounceSeconds)
            return;

        lastInvalidInteractTime[playerTag] = Time.time;
        AnalyticsManager.Instance?.LogEvent(actor, EventType.INVALID_INTERACT);
    }

    private void SetVisual(bool isOn)
    {
        if (leverVisual == null)
            return;

        if (isOn && leverOnSprite != null)
            leverVisual.sprite = leverOnSprite;
        else if (!isOn && leverOffSprite != null)
            leverVisual.sprite = leverOffSprite;
    }
}
