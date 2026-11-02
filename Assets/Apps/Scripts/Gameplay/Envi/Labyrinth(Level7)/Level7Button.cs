using UnityEngine;
using System.Collections;

public class Level7Button : MonoBehaviour
{
    public enum AllowedPlayer
    {
        Any,
        Player1,
        Player2
    }

    [Header("Button Settings")]
    [SerializeField] private int buttonID = 1;
    [SerializeField] private AllowedPlayer allowedPlayer =
        AllowedPlayer.Any;

    [Header("References")]
    [SerializeField] private Level7SequenceManager sequenceManager;
    [SerializeField] private Level7Door targetDoor;

    [Header("Button Visuals")]
    [SerializeField] private SpriteRenderer buttonVisual;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite activatedSprite;
    [SerializeField] private GameObject activatedVFX;

    [Header("Wrong Player Feedback")]
    [SerializeField] private GameObject wrongColorFeedback;
    [SerializeField] private float feedbackDuration = 0.5f;

    private bool isActivated;
    private Coroutine feedbackCoroutine;

    private void Start()
    {
        if (buttonVisual != null && normalSprite != null)
            buttonVisual.sprite = normalSprite;

        if (activatedVFX != null)
            activatedVFX.SetActive(false);

        if (wrongColorFeedback != null)
            wrongColorFeedback.SetActive(false);
    }

    public void Interact(GameObject actor)
    {
        if (actor == null || isActivated)
            return;

        bool isPlayer1 = actor.CompareTag("Player1");
        bool isPlayer2 = actor.CompareTag("Player2");

        if (!isPlayer1 && !isPlayer2)
            return;

        bool allowed =
            allowedPlayer == AllowedPlayer.Any ||
            (allowedPlayer == AllowedPlayer.Player1 && isPlayer1) ||
            (allowedPlayer == AllowedPlayer.Player2 && isPlayer2);

        if (!allowed)
        {
            LogInvalidInteraction(actor);
            ShowWrongColorFeedback();

            Debug.Log(actor.name + " cannot activate " + gameObject.name);
            return;
        }

        if (sequenceManager == null || targetDoor == null)
        {
            Debug.LogWarning(
                gameObject.name + ": Missing manager or target door."
            );
            return;
        }

        isActivated = true;

        targetDoor.OpenDoor();
        sequenceManager.RegisterButton(buttonID);

        if (buttonVisual != null && activatedSprite != null)
            buttonVisual.sprite = activatedSprite;

        if (activatedVFX != null)
            activatedVFX.SetActive(true);

        Debug.Log("Puzzle button " + buttonID + " activated.");
    }

    private void LogInvalidInteraction(GameObject actor)
    {
        if (AnalyticsManager.Instance == null)
            return;

        PlayerID playerID = actor.CompareTag("Player1")
            ? PlayerID.Player1
            : PlayerID.Player2;

        AnalyticsManager.Instance.LogEvent(
            playerID,
            EventType.INVALID_INTERACT
        );
    }

    private void ShowWrongColorFeedback()
    {
        if (wrongColorFeedback == null)
            return;

        if (feedbackCoroutine != null)
            StopCoroutine(feedbackCoroutine);

        feedbackCoroutine = StartCoroutine(WrongColorFeedbackRoutine());
    }

    private IEnumerator WrongColorFeedbackRoutine()
    {
        wrongColorFeedback.SetActive(true);

        yield return new WaitForSeconds(feedbackDuration);

        wrongColorFeedback.SetActive(false);
        feedbackCoroutine = null;
    }
}