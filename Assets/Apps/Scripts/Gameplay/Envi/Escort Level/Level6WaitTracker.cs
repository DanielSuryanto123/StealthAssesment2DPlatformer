
using UnityEngine;

[RequireComponent(typeof(InputHandler))]
public class Level6WaitTracker : MonoBehaviour
{
    [SerializeField] private float movementThreshold = 0.1f;
    [SerializeField] private float waitDurationThreshold = 3f;
    [SerializeField] private bool isTrapped;

    private InputHandler inputHandler;
    private Transform partner;
    private PlayerID playerID;

    private bool wasMoving;
    private bool isWaiting;
    private float waitStartTime;

    private void Awake()
    {
        inputHandler = GetComponent<InputHandler>();

        if (CompareTag("Player1"))
        {
            playerID = PlayerID.Player1;
        }
        else if (CompareTag("Player2"))
        {
            playerID = PlayerID.Player2;
        }
        else
        {
            Debug.LogError(
                $"{name} must have Player1 or Player2 tag."
            );
        }
    }

    private void Start()
    {
        AnalyticsManager manager = AnalyticsManager.Instance;

        if (manager != null)
        {
            manager.SetPlayers(
                GameObject.FindGameObjectWithTag("Player1")?.transform,
                GameObject.FindGameObjectWithTag("Player2")?.transform
            );
        }

        string partnerTag = playerID == PlayerID.Player1
            ? "Player2"
            : "Player1";

        GameObject partnerObject =
            GameObject.FindGameObjectWithTag(partnerTag);

        if (partnerObject != null)
            partner = partnerObject.transform;
    }

    private void Update()
    {
        bool isMoving =
            Mathf.Abs(inputHandler.MoveInput.x) > movementThreshold;

        if (isMoving && !wasMoving)
        {
            AnalyticsManager.Instance?.LogEvent(
                playerID, EventType.MOVE_START, isTrapped
            );
        }
        else if (!isMoving && wasMoving)
        {
            AnalyticsManager.Instance?.LogEvent(
                playerID, EventType.MOVE_STOP, isTrapped
            );
        }

        wasMoving = isMoving;

        bool partnerMoving = false;

        if (partner != null)
        {
            InputHandler partnerInput =
                partner.GetComponent<InputHandler>();

            if (partnerInput != null)
            {
                partnerMoving =
                    Mathf.Abs(partnerInput.MoveInput.x) >
                    movementThreshold;
            }
        }

        bool shouldWait = !isMoving && partnerMoving;

        if (shouldWait && !isWaiting)
        {
            isWaiting = true;
            waitStartTime = Time.time;
        }
        else if (!shouldWait && isWaiting)
        {
            float duration = Time.time - waitStartTime;

            if (duration > waitDurationThreshold)
            {
                AnalyticsManager.Instance?.LogEvent(
                    playerID,
                    EventType.WAIT_PARTNER,
                    isTrapped,
                    duration
                );
            }

            isWaiting = false;
        }
    }

    private void OnDisable()
    {
        if (!isWaiting)
            return;

        float duration = Time.time - waitStartTime;

        if (duration > waitDurationThreshold &&
            AnalyticsManager.Instance != null)
        {
            AnalyticsManager.Instance.LogEvent(
                playerID,
                EventType.WAIT_PARTNER,
                isTrapped,
                duration
            );
        }

        isWaiting = false;
    }
}
