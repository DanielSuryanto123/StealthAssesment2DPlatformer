using UnityEngine;

public class Level6DeadzoneLever : MonoBehaviour
{
    [Header("Rescue")]
    [SerializeField] private Level6DeadzoneTrigger targetDeadzone;
    [SerializeField] private bool singleUse = true;

    private bool isActivated;
    private LeverController leverController;

    private void Awake()
    {
        leverController = GetComponent<LeverController>();
    }

    private void OnEnable()
    {
        if (leverController == null)
            leverController = GetComponent<LeverController>();

        if (leverController == null)
        {
            Debug.LogWarning(gameObject.name + ": Level6DeadzoneLever needs a sibling LeverController.");
            return;
        }

        leverController.Activated += ActivateLever;
    }

    private void OnDisable()
    {
        if (leverController != null)
            leverController.Activated -= ActivateLever;
    }

    public void ActivateLever(Transform actor)
    {
        if (actor == null || !actor.CompareTag("Player2"))
            return;

        if (singleUse && isActivated)
            return;

        if (targetDeadzone == null)
        {
            Debug.LogWarning("Target Deadzone has not been assigned.");
            return;
        }

        isActivated = true;

        // PULL_LEVER is logged once by the sibling LeverController.
        targetDeadzone.ReleasePlayer(actor);

        Debug.Log(gameObject.name + " activated by Player2.");
    }

    public void ResetLever()
    {
        isActivated = false;
    }
}