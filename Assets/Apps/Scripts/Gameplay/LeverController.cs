using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A lever/switch that toggles one or more LaserHazard components.
/// Only activates when touched by the player whose tag matches this
/// lever's required tag ("Player1" = punk/red, "Player2" = cyborg/blue) —
/// supports the classic "Red lever unlocks Blue's path" co-op puzzle mechanic.
///
/// Setup:
/// - Attach to the lever/switch GameObject.
/// - Add a Collider2D set to "Is Trigger" covering the interaction zone.
/// - Assign the LaserHazard(s) this lever controls in "Linked Hazards".
/// - Set "Required Player Tag" to "Player1" or "Player2".
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class LeverController : MonoBehaviour
{
    [Tooltip("Only a player with this tag can activate the lever. Use \"Player1\" (punk/red) or \"Player2\" (cyborg/blue).")]
    [SerializeField] private string requiredPlayerTag = "Player1";

    [Tooltip("The hazards this lever controls.")]
    [SerializeField] private List<LaserHazard> linkedHazards = new List<LaserHazard>();

    [Tooltip("If true, activating the lever always deactivates the hazards (one-way switch). If false, each activation toggles current state.")]
    [SerializeField] private bool oneWayDeactivateOnly = true;

    [Tooltip("If true, the lever can only be used once and then locks.")]
    [SerializeField] private bool singleUse = false;

    [Header("Visuals (optional)")]
    [SerializeField] private SpriteRenderer leverVisual;
    [SerializeField] private Sprite leverOnSprite;
    [SerializeField] private Sprite leverOffSprite;
    [SerializeField] private GameObject activationVfx;
    [SerializeField] private GameObject wrongColorFeedback;

    private bool hasBeenUsed = false;
    private bool leverIsOn = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (singleUse && hasBeenUsed) return;

        if (!other.CompareTag(requiredPlayerTag))
        {
            // Either the wrong player, or not a player at all.
            if ((other.CompareTag("Player1") || other.CompareTag("Player2")) && wrongColorFeedback != null)
            {
                Instantiate(wrongColorFeedback, transform.position, Quaternion.identity);
            }
            return;
        }

        Activate();
    }

    public void Activate()
    {
        hasBeenUsed = true;

        bool newHazardState;

        if (oneWayDeactivateOnly)
        {
            leverIsOn = true;
            newHazardState = false;
        }
        else
        {
            leverIsOn = !leverIsOn;
            newHazardState = !leverIsOn;
        }

        foreach (var hazard in linkedHazards)
        {
            if (hazard != null)
            {
                hazard.SetActive(newHazardState);
            }
        }

        UpdateVisuals();

        if (activationVfx != null)
        {
            Instantiate(activationVfx, transform.position, Quaternion.identity);
        }
    }

    private void UpdateVisuals()
    {
        if (leverVisual == null) return;

        if (leverIsOn && leverOnSprite != null)
        {
            leverVisual.sprite = leverOnSprite;
        }
        else if (!leverIsOn && leverOffSprite != null)
        {
            leverVisual.sprite = leverOffSprite;
        }
    }
}