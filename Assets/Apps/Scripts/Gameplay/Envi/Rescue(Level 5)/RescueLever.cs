using System.Collections.Generic;
using UnityEngine;

public class RescueLever : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PitfallRescueManager rescueManager;

    [Header("Interaction")]
    [SerializeField] private string requiredPlayerTag = "Player1";

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer leverVisual;
    [SerializeField] private Sprite leverOffSprite;
    [SerializeField] private Sprite leverOnSprite;

    private readonly Dictionary<string, InputHandler> playersNearby = new();
    private readonly Dictionary<string, GameObject> nearbyPlayers = new();

    private bool activated;

    private void Start()
    {
        SetVisual(false);
    }

    private void Update()
    {
        if (activated)
            return;

        if (!playersNearby.TryGetValue(requiredPlayerTag, out InputHandler input))
            return;

        if (input == null || !input.InteractPressed)
            return;

        if (!nearbyPlayers.TryGetValue(requiredPlayerTag, out GameObject player))
            return;

        if (rescueManager == null)
        {
            Debug.LogError("RescueLever: Rescue Manager is missing.");
            return;
        }

        input.ResetInteract();

        rescueManager.ActivateRescue(player);

        if (rescueManager.IsRescueCompleted)
        {
            activated = true;
            SetVisual(true);

            Debug.Log(gameObject.name + " activated by " + requiredPlayerTag + ".");
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
        nearbyPlayers[playerTag] = playerRoot.gameObject;

        Debug.Log(playerRoot.name + " entered Rescue Lever. Tag: " + playerTag);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Transform playerRoot = other.transform.root;
        string playerTag = playerRoot.tag;

        if (playerTag == "Player1" || playerTag == "Player2")
        {
            playersNearby.Remove(playerTag);
            nearbyPlayers.Remove(playerTag);

            Debug.Log(playerRoot.name + " exited Rescue Lever.");
        }
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

    public void ResetLever()
    {
        activated = false;
        SetVisual(false);
    }
}

