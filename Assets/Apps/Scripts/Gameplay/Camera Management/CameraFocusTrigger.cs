
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CameraFocusTrigger : MonoBehaviour
{
    [SerializeField] private TargetGroupController targetGroupController;
    [SerializeField] private string requiredPlayerTag = "Player1";
    [SerializeField] private bool triggerOnce = true;

    private bool hasTriggered;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered && triggerOnce)
            return;

        Transform player = other.transform;

        if (!player.CompareTag(requiredPlayerTag))
            return;

        if (targetGroupController == null)
        {
            Debug.LogWarning("CameraFocusTrigger: TargetGroupController is missing.");
            return;
        }

        targetGroupController.FocusOnPlayer(player);
        hasTriggered = true;
    }
}
