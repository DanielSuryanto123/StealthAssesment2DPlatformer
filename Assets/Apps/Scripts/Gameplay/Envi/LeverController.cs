
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LeverController : MonoBehaviour
{
    [Header("Player Access")]
    [SerializeField] private string requiredPlayerTag = "Player1";

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

    public string RequiredPlayerTag => requiredPlayerTag;
    public bool IsActivated => activated;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        UpdateVisual(false);
    }

    private void Update()
    {
        if (singleUse && activated)
            return;

        foreach (var entry in playersNearby)
        {
            InputHandler input = entry.Value;

            if (input == null || !input.InteractPressed)
                continue;

            if (entry.Key != requiredPlayerTag)
            {
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

            ActivateLever();
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

    private void ActivateLever()
    {
        if (activated)
            return;

        activated = true;
        UpdateVisual(true);

        foreach (LaserHazard hazard in linkedHazards)
        {
            if (hazard != null)
                hazard.SetActive(!deactivateHazardsOnUse);
        }

        if (activationVfx != null)
            Instantiate(activationVfx, transform.position, Quaternion.identity);

        if (coopPuzzle != null)
            coopPuzzle.RegisterLever(requiredPlayerTag, this);
    }

    public void ResetLever()
    {
        activated = false;
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
