using UnityEngine;
using UnityEngine.SceneManagement;
using CoLab.Telemetry;
using EventType = CoLab.Telemetry.EventType;

/// <summary>
/// Logs MOVE_START / MOVE_STOP on horizontal-input edges and WAIT_PARTNER when this player
/// idles for longer than waitDurationThreshold while the partner is moving.
/// Attached automatically to Player1/Player2 in "Level*" scenes (see Attach).
/// </summary>
[RequireComponent(typeof(InputHandler))]
public class PlayerActivityTracker : MonoBehaviour
{
    [SerializeField] protected float movementThreshold = 0.1f;
    [SerializeField] protected float waitDurationThreshold = 3f;
    // legacy, ignored (central trapped state used)
    [SerializeField] protected bool isTrapped;

    private static bool bootstrapped;

    private InputHandler inputHandler;
    private InputHandler partnerInput;
    private PlayerID playerID;

    private bool wasMoving;
    private bool isWaiting;
    private float waitStartTime;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (!bootstrapped)
        {
            bootstrapped = true;
            SceneManager.sceneLoaded += (scene, mode) => Attach();
        }

        Attach();
    }

    private static void Attach()
    {
        if (!SceneManager.GetActiveScene().name.StartsWith("Level"))
            return;

        AttachToTag("Player1");
        AttachToTag("Player2");
    }

    private static void AttachToTag(string playerTag)
    {
        foreach (GameObject player in GameObject.FindGameObjectsWithTag(playerTag))
        {
            if (player.GetComponent<InputHandler>() == null)
                continue;

            if (player.GetComponent<PlayerActivityTracker>() == null)
                player.AddComponent<PlayerActivityTracker>();
        }
    }

    protected virtual void Awake()
    {
        inputHandler = GetComponent<InputHandler>();

        if (!AnalyticsManager.TryGetPlayerID(transform, out playerID))
        {
            Debug.LogError($"{name} must have Player1 or Player2 tag.");
            enabled = false;
        }
    }

    protected virtual void Update()
    {
        bool isMoving =
            Mathf.Abs(inputHandler.MoveInput.x) > movementThreshold;

        if (isMoving && !wasMoving)
        {
            AnalyticsManager.Instance?.LogEvent(playerID, EventType.MOVE_START, false);
        }
        else if (!isMoving && wasMoving)
        {
            AnalyticsManager.Instance?.LogEvent(playerID, EventType.MOVE_STOP, false);
        }

        wasMoving = isMoving;

        if (partnerInput == null)
        {
            string partnerTag = playerID == PlayerID.PLAYER_1 ? "Player2" : "Player1";
            GameObject partnerObject = GameObject.FindGameObjectWithTag(partnerTag);

            if (partnerObject != null)
                partnerInput = partnerObject.GetComponent<InputHandler>();
        }

        bool partnerMoving =
            partnerInput != null &&
            Mathf.Abs(partnerInput.MoveInput.x) > movementThreshold;

        bool shouldWait = !isMoving && partnerMoving;

        if (shouldWait && !isWaiting)
        {
            isWaiting = true;
            waitStartTime = Time.time;
        }
        else if (!shouldWait && isWaiting)
        {
            LogWait();
        }
    }

    protected virtual void OnDisable()
    {
        // Skip the flush on scene unload so it is not stamped into the next stage.
        if (isWaiting && gameObject.scene.isLoaded)
            LogWait();

        isWaiting = false;
    }

    private void LogWait()
    {
        float duration = Time.time - waitStartTime;
        isWaiting = false;

        if (duration > waitDurationThreshold)
        {
            AnalyticsManager.Instance?.LogEvent(
                playerID, EventType.WAIT_PARTNER, false, duration
            );
        }
    }
}
