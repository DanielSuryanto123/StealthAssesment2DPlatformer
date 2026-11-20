using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Level6PlatformCallButton : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private bool player1Only;
    [SerializeField] private bool player2Only;

    [Header("Platform")]
    [SerializeField] private Level6MovingPlatform targetPlatform;

    [Header("Floor Selection")]
    [SerializeField] private int boardingFloorIndex = 0;
    [SerializeField] private int destinationIndex = 1;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer buttonRenderer;
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite pressedSprite;
    [SerializeField] private GameObject activationVfx;
    [SerializeField] private GameObject wrongPlayerFeedback;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip activationSound;

    private readonly Dictionary<string, InputHandler> nearbyPlayers =
        new Dictionary<string, InputHandler>();

    private readonly Dictionary<string, int> contactCount =
        new Dictionary<string, int>();

    private bool isPressed;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        UpdateVisual();
    }

    private void Update()
    {
        foreach (var player in nearbyPlayers)
        {
            InputHandler input = player.Value;

            if (input == null || !input.InteractPressed)
                continue;

            input.ResetInteract();

            if (!CanPlayerUseButton(player.Key))
            {
                ShowWrongPlayerFeedback();
                continue;
            }

            Interact(input.transform.root.gameObject);
            break;
        }
    }

    private bool CanPlayerUseButton(string playerTag)
    {
        if (!player1Only && !player2Only)
            return true;

        if (player1Only && !player2Only)
            return playerTag == "Player1";

        if (player2Only && !player1Only)
            return playerTag == "Player2";

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

        RegisterPlayer(other, playerTag);
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

    private void RegisterPlayer(Collider2D other, string playerTag)
    {
        Transform playerRoot = other.transform.root;

        InputHandler input = playerRoot.GetComponent<InputHandler>();

        if (input == null)
            input = other.GetComponentInParent<InputHandler>();

        if (input == null)
        {
            Debug.LogWarning(playerRoot.name + " has no InputHandler!");
            return;
        }

        nearbyPlayers[playerTag] = input;
    }

    private string GetPlayerTag(Collider2D other)
    {
        Transform playerRoot = other.transform.root;

        if (playerRoot.CompareTag("Player1"))
            return "Player1";

        if (playerRoot.CompareTag("Player2"))
            return "Player2";

        return null;
    }

    public void Interact(GameObject actor)
    {
        if (actor == null)
            return;

        if (!actor.CompareTag("Player1") &&
            !actor.CompareTag("Player2"))
            return;

        if (!CanPlayerUseButton(actor.tag))
        {
            ShowWrongPlayerFeedback();
            return;
        }

        if (targetPlatform == null)
        {
            Debug.LogWarning("Level 6 platform is not assigned.");
            return;
        }

        targetPlatform.RequestTrip(
            boardingFloorIndex,
            destinationIndex
        );

        PlayFeedback();

        Debug.Log(
            actor.name + " requested Level 6 platform floor " +
            destinationIndex
        );
    }

    private void PlayFeedback()
    {
        isPressed = true;
        UpdateVisual();

        if (activationVfx != null)
        {
            Instantiate(
                activationVfx,
                transform.position,
                Quaternion.identity
            );
        }

        if (audioSource != null && activationSound != null)
        {
            audioSource.PlayOneShot(activationSound);
        }
    }

    private void UpdateVisual()
    {
        if (buttonRenderer == null)
            return;

        buttonRenderer.sprite = isPressed
            ? pressedSprite
            : idleSprite;
    }

    private void ShowWrongPlayerFeedback()
    {
        if (wrongPlayerFeedback == null)
            return;

        GameObject feedback = Instantiate(
            wrongPlayerFeedback,
            transform.position,
            Quaternion.identity
        );

        Destroy(feedback, 1f);
    }

    public void ResetButton()
    {
        isPressed = false;
        UpdateVisual();
    }
}
