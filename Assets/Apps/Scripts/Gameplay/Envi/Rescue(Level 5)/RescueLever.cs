
using UnityEngine;
using UnityEngine.InputSystem;

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
    private bool activated;

    private void Start()
    {
        SetVisual(false);
    }

    private void Update()
    {
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
            return;

        nearbyPlayer = player;
        nearbyInput = player.GetComponent<InputHandler>();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerInput playerInput = other.GetComponentInParent<PlayerInput>();

        if (playerInput == null || nearbyPlayer != playerInput.gameObject)
            return;

        nearbyPlayer = null;
        nearbyInput = null;
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
