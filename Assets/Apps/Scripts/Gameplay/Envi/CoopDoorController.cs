
using UnityEngine;

public class CoopDoorController : MonoBehaviour
{
    [Header("Door")]
    [SerializeField] private GameObject door;

    [Header("Co-op Settings")]
    [SerializeField] private float activationWindowSeconds = 2f;

    private LeverController player1Lever;
    private LeverController player2Lever;

    private bool player1Activated;
    private bool player2Activated;
    private bool puzzleCompleted;
    private float firstActivationTime;

    private void Start()
    {
        ResetPuzzle();
    }

    private void Update()
    {
        if (puzzleCompleted)
            return;

        if ((player1Activated || player2Activated) &&
            Time.time - firstActivationTime > activationWindowSeconds)
        {
            ResetPuzzle();
            Debug.Log("Co-op puzzle timed out. Levers reset.");
        }
    }

    public void RegisterLever(string playerTag, LeverController lever)
    {
        if (puzzleCompleted || lever == null)
            return;

        if (playerTag != lever.RequiredPlayerTag)
            return;

        if (player1Activated || player2Activated)
        {
            if (Time.time - firstActivationTime > activationWindowSeconds)
                ResetPuzzle();
        }

        if (!player1Activated && !player2Activated)
            firstActivationTime = Time.time;

        if (playerTag == "Player1")
        {
            player1Activated = true;
            player1Lever = lever;
        }
        else if (playerTag == "Player2")
        {
            player2Activated = true;
            player2Lever = lever;
        }
        else
        {
            return;
        }

        if (player1Activated && player2Activated)
        {
            puzzleCompleted = true;

            if (door != null)
                door.SetActive(false);

            Debug.Log("Co-op puzzle completed! Door opened.");
        }
    }

    private void ResetPuzzle()
    {
        player1Activated = false;
        player2Activated = false;

        LeverController first = player1Lever;
        LeverController second = player2Lever;

        player1Lever = null;
        player2Lever = null;

        if (first != null)
            first.ResetLever();

        if (second != null && second != first)
            second.ResetLever();

        if (door != null)
            door.SetActive(true);
    }
}
