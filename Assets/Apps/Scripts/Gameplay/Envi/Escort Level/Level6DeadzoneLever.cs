using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Level6DeadzoneLever : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private bool player1Only;
    [SerializeField] private bool player2Only = true;

    [Header("Rescue")]
    [SerializeField] private Level6DeadzoneTrigger targetDeadzone;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer leverRenderer;
    [SerializeField] private Sprite offSprite;
    [SerializeField] private Sprite onSprite;
    [SerializeField] private GameObject wrongColorFeedback;
    [SerializeField] private GameObject activationVfx;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip activationSound;

    private readonly Dictionary<string, InputHandler> nearbyPlayers =
        new Dictionary<string, InputHandler>();

    private readonly Dictionary<string, int> contactCount =
        new Dictionary<string, int>();

    private bool activated;

    public bool IsActivated => activated;

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

            if (targetDeadzone == null)
            {
                Debug.LogWarning("Target Deadzone has not been assigned.");
                continue;
            }

            if (!targetDeadzone.IsTrapped)
            {
                ShowWrongColorFeedback();
                continue;
            }

            ActivateLever(input.transform.root);
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
        Debug.Log(playerRoot.name + " entered deadzone lever range.");
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

    private void ActivateLever(Transform actor)
    {
        if (activated || actor == null)
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

        if (audioSource != null && activationSound != null)
        {
            audioSource.PlayOneShot(activationSound);
        }

        targetDeadzone.ReleasePlayer(actor);

        Debug.Log(name + " successfully rescued Player1!");
    }

    public void ResetLever()
    {
        activated = false;
        UpdateVisual();
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
