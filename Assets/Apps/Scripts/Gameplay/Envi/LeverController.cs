
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LeverController : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private bool player1Only;
    [SerializeField] private bool player2Only;

    [Header("Door")]
    [SerializeField] private SinglePlayerDoorController singlePlayerDoor;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer leverRenderer;
    [SerializeField] private Sprite offSprite;
    [SerializeField] private Sprite onSprite;
    [SerializeField] private GameObject wrongColorFeedback;
    [SerializeField] private GameObject activationVfx;

    private readonly Dictionary<string, InputHandler> nearbyPlayers =
        new Dictionary<string, InputHandler>();

    private readonly Dictionary<string, int> contactCount =
        new Dictionary<string, int>();

    private bool activated;

    public bool IsActivated => activated;

    // Notifies other scripts when a player successfully activates this lever.
    public event Action<Transform> Activated;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        UpdateVisual();
    }

    private void Update()
    {
        if (activated)
            return;

        foreach (var player in nearbyPlayers)
        {
            InputHandler input = player.Value;

            if (input == null || !input.InteractPressed)
                continue;

            input.ResetInteract();

            if (!CanPlayerUseLever(player.Key))
            {
                ShowWrongColorFeedback();
                continue;
            }

            Transform actor = input.transform.root;
            ActivateLever(actor);
            break;
        }
    }

    private bool CanPlayerUseLever(string playerTag)
    {
        if (!player1Only && !player2Only)
            return true;

        if (player1Only && !player2Only)
            return playerTag == "Player1";

        if (player2Only && !player1Only)
            return playerTag == "Player2";

        // Both checked: neither player is allowed.
        return false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        string playerTag = GetPlayerTag(other);

        if (string.IsNullOrEmpty(playerTag))
            return;

        bool firstContact = !contactCount.ContainsKey(playerTag);

        contactCount[playerTag] = firstContact
            ? 1
            : contactCount[playerTag] + 1;

        RegisterPlayer(other, playerTag, firstContact);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        string playerTag = GetPlayerTag(other);

        if (string.IsNullOrEmpty(playerTag))
            return;

        if (!contactCount.TryGetValue(playerTag, out int count))
            return;

        count--;

        if (count <= 0)
        {
            contactCount.Remove(playerTag);
            nearbyPlayers.Remove(playerTag);
        }
        else
        {
            contactCount[playerTag] = count;
        }
    }

    private void RegisterPlayer(
        Collider2D other,
        string playerTag,
        bool firstContact)
    {
        Transform playerRoot = other.transform.root;

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

        nearbyPlayers[playerTag] = input;

        Debug.Log(
            playerRoot.name + " entered lever. Tag: " + playerTag
        );
    }

    private string GetPlayerTag(Collider2D other)
    {
        Transform playerRoot = other.transform.root;
        string playerTag = playerRoot.tag;

        if (playerTag == "Player1" || playerTag == "Player2")
            return playerTag;

        return null;
    }

    private void ActivateLever(Transform actor)
    {
        if (activated)
            return;

        activated = true;

        UpdateVisual();

        if (activationVfx != null)
        {
            Instantiate(
                activationVfx,
                transform.position,
                Quaternion.identity
            );
        }

        if (singlePlayerDoor != null)
            singlePlayerDoor.OpenDoor();

        // Notify scripts listening to this lever.
        Activated?.Invoke(actor);

        Debug.Log(name + " activated permanently!");
    }

    public void ResetLever()
    {
        activated = false;

        UpdateVisual();

        if (singlePlayerDoor != null)
            singlePlayerDoor.CloseDoor();
    }

    private void UpdateVisual()
    {
        if (leverRenderer == null)
            return;

        leverRenderer.sprite = activated
            ? onSprite
            : offSprite;
    }

    private void ShowWrongColorFeedback()
    {
        if (wrongColorFeedback == null)
            return;

        GameObject feedback = Instantiate(
            wrongColorFeedback,
            transform.position,
            Quaternion.identity
        );

        Destroy(feedback, 1f);
    }
}
