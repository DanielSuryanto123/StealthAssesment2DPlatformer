using UnityEngine;

[RequireComponent(typeof(InputHandler))]
public class Level7PlayerInteraction : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private float interactionRadius = 1f;
    [SerializeField] private LayerMask interactableLayer;

    private InputHandler inputHandler;

    private void Awake()
    {
        inputHandler = GetComponent<InputHandler>();
    }

    private void Update()
    {
        if (!inputHandler.InteractPressed)
            return;

        TryInteract();
        inputHandler.ResetInteract();
    }

    private void TryInteract()
    {
        Vector2 position = interactionPoint != null
            ? interactionPoint.position
            : transform.position;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            position,
            interactionRadius,
            interactableLayer
        );

        foreach (Collider2D hit in hits)
        {
            Level7Button puzzleButton =
                hit.GetComponentInParent<Level7Button>();

            if (puzzleButton != null)
            {
                puzzleButton.Interact(gameObject);
                return;
            }

            Level7PlatformCallButton platformButton =
                hit.GetComponentInParent<Level7PlatformCallButton>();

            if (platformButton != null)
            {
                platformButton.Interact(gameObject);
                return;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 position = interactionPoint != null
            ? interactionPoint.position
            : transform.position;

        Gizmos.DrawWireSphere(position, interactionRadius);
    }
}