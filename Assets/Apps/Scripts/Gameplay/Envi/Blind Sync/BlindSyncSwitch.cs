
using System.Collections.Generic;
using UnityEngine;

public class BlindSyncSwitch : MonoBehaviour
{
    [Header("Switch Settings")]
    [SerializeField] private string requiredPlayerTag = "Player1";
    [SerializeField] private BlindSyncManager syncManager;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer switchRenderer;
    [SerializeField] private Color inactiveColor = Color.gray;
    [SerializeField] private Color activeColor = Color.green;

    private readonly Dictionary<string, InputHandler> playersNearby = new();
    private bool activated;

    private void Start()
    {
        UpdateVisual();
    }

    private void Update()
    {
        if (activated || syncManager == null || syncManager.IsCompleted)
            return;

        if (playersNearby.TryGetValue(requiredPlayerTag, out InputHandler input) &&
            input != null && input.InteractPressed)
        {
            activated = true;
            UpdateVisual();
            syncManager.RegisterSwitch(requiredPlayerTag);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        InputHandler input = other.GetComponent<InputHandler>();

        if (input == null)
            input = other.GetComponentInParent<InputHandler>();

        if (input == null)
            return;

        if (other.CompareTag("Player1"))
            playersNearby["Player1"] = input;
        else if (other.CompareTag("Player2"))
            playersNearby["Player2"] = input;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player1"))
            playersNearby.Remove("Player1");
        else if (other.CompareTag("Player2"))
            playersNearby.Remove("Player2");
    }

    public void ResetSwitch()
    {
        activated = false;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (switchRenderer != null)
            switchRenderer.color = activated ? activeColor : inactiveColor;
    }
}
