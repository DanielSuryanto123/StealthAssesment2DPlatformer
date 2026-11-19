using System.Collections.Generic;
using CoLab.Telemetry;
using UnityEngine;
using EventType = CoLab.Telemetry.EventType;

public class RescueLever : MonoBehaviour
{
    [Header("Rescue Settings")]
    [SerializeField] private string requiredPlayerTag = "Player1";
    [SerializeField] private PitfallRescueManager rescueManager;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer leverVisual;
    [SerializeField] private Sprite leverOffSprite;
    [SerializeField] private Sprite leverOnSprite;

    private readonly Dictionary<string, InputHandler> playersNearby = new();

    private bool activated;

    private const float InvalidInteractDebounceSeconds = 0.25f;

    private readonly Dictionary<string, float> lastInvalidInteractTime =
        new();

    private void Start()
    {
        UpdateVisual();
    }

    private void Update()
    {
        if (activated || rescueManager == null)
            return;

        string wrongTag = requiredPlayerTag == "Player1"
            ? "Player2"
            : "Player1";

        if (playersNearby.TryGetValue(wrongTag, out InputHandler wrongInput) &&
            wrongInput != null && wrongInput.InteractPressed)
        {
            LogInvalidInteract(wrongTag, wrongInput.transform);
            wrongInput.ResetInteract();
        }

        if (playersNearby.TryGetValue(requiredPlayerTag, out InputHandler input) &&
            input != null && input.InteractPressed)
        {
            input.ResetInteract();

            GameObject player = input.transform.root.gameObject;

            Debug.Log("RescueLever: Interaction received from " + player.tag);

            rescueManager.ActivateRescue(player);

            if (rescueManager.IsRescueCompleted)
            {
                activated = true;
                UpdateVisual();

                AnalyticsManager.Instance?.LogEvent(
                    player.transform,
                    EventType.PULL_LEVER
                );

                Debug.Log("RescueLever: Rescue completed!");
            }
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
            Debug.LogWarning(
                playerRoot.name + " has no InputHandler!"
            );
            return;
        }

        playersNearby[playerTag] = input;

        Debug.Log(
            playerRoot.name + " entered RescueLever. Tag: " + playerTag
        );
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Transform playerRoot = other.transform.root;

        string playerTag = playerRoot.tag;

        if (playerTag == "Player1" || playerTag == "Player2")
        {
            playersNearby.Remove(playerTag);

            Debug.Log(
                playerRoot.name + " exited RescueLever."
            );
        }
    }

    private void LogInvalidInteract(string playerTag, Transform actor)
    {
        if (lastInvalidInteractTime.TryGetValue(playerTag, out float last) &&
            Time.time - last < InvalidInteractDebounceSeconds)
            return;

        lastInvalidInteractTime[playerTag] = Time.time;

        AnalyticsManager.Instance?.LogEvent(
            actor,
            EventType.INVALID_INTERACT
        );
    }

    public void ResetLever()
    {
        activated = false;
        UpdateVisual();
    }

    public void LockLever()
    {
        activated = true;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (leverVisual == null)
            return;

        leverVisual.sprite = activated
            ? leverOnSprite
            : leverOffSprite;
    }
}
