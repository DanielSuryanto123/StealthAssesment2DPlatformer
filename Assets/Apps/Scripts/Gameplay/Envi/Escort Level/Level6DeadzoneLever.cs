using System.Collections.Generic;
using UnityEngine;
using CoLab.Telemetry;

public class Level6DeadzoneLever : MonoBehaviour
{
    [Header("Rescue")]
    [SerializeField] private Level6DeadzoneTrigger targetDeadzone;
    [SerializeField] private bool singleUse = true;
    [SerializeField] private string requiredPlayerTag = "Player2";

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer leverRenderer;
    [SerializeField] private Sprite offSprite;
    [SerializeField] private Sprite onSprite;

    private readonly Dictionary<string, InputHandler> playersNearby = new();

    private bool isActivated;

    private void Start()
    {
        UpdateVisual();
    }

    private void Update()
    {
        if (singleUse && isActivated)
            return;

        if (!playersNearby.TryGetValue(requiredPlayerTag, out InputHandler input))
            return;

        if (input == null || !input.InteractPressed)
            return;

        input.ResetInteract();

        ActivateLever();
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

        Debug.Log(playerRoot.name + " entered Deadzone Lever. Tag: " + playerTag);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Transform playerRoot = other.transform.root;
        string playerTag = playerRoot.tag;

        if (playerTag == "Player1" || playerTag == "Player2")
        {
            playersNearby.Remove(playerTag);
            Debug.Log(playerRoot.name + " exited Deadzone Lever.");
        }
    }

    private void ActivateLever()
    {
        if (singleUse && isActivated)
            return;

        if (targetDeadzone == null)
        {
            Debug.LogWarning("Target Deadzone has not been assigned.");
            return;
        }

        isActivated = true;

        targetDeadzone.ReleasePlayer();

        UpdateVisual();

        if (AnalyticsManager.Instance != null)
        {
            AnalyticsManager.Instance.LogLeverPull(transform);
        }

        Debug.Log(gameObject.name + " activated by " + requiredPlayerTag + ".");
    }

    public void ResetLever()
    {
        isActivated = false;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (leverRenderer == null)
            return;

        leverRenderer.sprite = isActivated ? onSprite : offSprite;
    }
}


