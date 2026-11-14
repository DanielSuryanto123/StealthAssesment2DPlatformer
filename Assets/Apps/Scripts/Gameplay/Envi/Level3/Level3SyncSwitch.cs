
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Level3SyncSwitch : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private string requiredPlayerTag = "Player1";

    [Header("Sync Manager")]
    [SerializeField] private Level3SyncManager syncManager;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer switchRenderer;
    [SerializeField] private Sprite inactiveSprite;
    [SerializeField] private Sprite activeSprite;

    private bool isActivated;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;

        if (switchRenderer != null && inactiveSprite != null)
            switchRenderer.sprite = inactiveSprite;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isActivated)
            return;

        if (!other.CompareTag(requiredPlayerTag))
            return;

        isActivated = true;

        if (switchRenderer != null && activeSprite != null)
            switchRenderer.sprite = activeSprite;

        if (syncManager != null)
        {
            syncManager.RecordButtonEvent("STEP_ON_BUTTON", requiredPlayerTag);
            syncManager.RegisterSwitch(requiredPlayerTag);
        }

        Debug.Log(requiredPlayerTag + " stepped on their switch.");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(requiredPlayerTag))
            return;

        if (syncManager != null)
            syncManager.RecordButtonEvent("STEP_OFF_BUTTON", requiredPlayerTag);
    }
}
