using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Level7MovingPlatform : MonoBehaviour
{
    private enum ElevatorState
    {
        Idle,
        MovingToBoardingFloor,
        WaitingForPlayers,
        MovingToDestination
    }

    [Header("Platform Stops")]
    [SerializeField] private Transform[] stoppingPoints;
    [SerializeField] private int startingPointIndex = 0;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stoppingDistance = 0.05f;

    private Rigidbody2D rb;
    private ElevatorState state = ElevatorState.Idle;

    private int currentPointIndex;
    private int boardingPointIndex;
    private int destinationPointIndex;

    private readonly HashSet<Rigidbody2D> passengers =
        new HashSet<Rigidbody2D>();

    public bool IsMoving =>
        state == ElevatorState.MovingToBoardingFloor ||
        state == ElevatorState.MovingToDestination;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        if (stoppingPoints == null || stoppingPoints.Length == 0)
        {
            Debug.LogWarning("Elevator has no stopping points.");
            enabled = false;
            return;
        }

        startingPointIndex = Mathf.Clamp(
            startingPointIndex, 0, stoppingPoints.Length - 1);

        currentPointIndex = startingPointIndex;

        if (stoppingPoints[currentPointIndex] != null)
        {
            rb.position = stoppingPoints[currentPointIndex].position;
        }
    }

    private void FixedUpdate()
    {
        passengers.RemoveWhere(p => p == null);

        if (state == ElevatorState.MovingToBoardingFloor)
        {
            MoveToSelectedPoint(boardingPointIndex);

            if (ReachedPoint(boardingPointIndex))
            {
                state = ElevatorState.WaitingForPlayers;
                Debug.Log("Elevator is waiting for both players.");
            }
        }
        else if (state == ElevatorState.WaitingForPlayers)
        {
            if (HasBothPlayersBoarded())
            {
                passengers.Clear();
                state = ElevatorState.MovingToDestination;

                Debug.Log("Both players boarded. Elevator moving.");
            }
        }
        else if (state == ElevatorState.MovingToDestination)
        {
            MoveToSelectedPoint(destinationPointIndex);

            if (ReachedPoint(destinationPointIndex))
            {
                state = ElevatorState.Idle;
                passengers.Clear();

                Debug.Log("Elevator reached its destination.");
            }
        }
    }

    public void RequestTrip(int boardingFloor, int destinationFloor)
    {
        if (state != ElevatorState.Idle)
        {
            Debug.Log("Elevator is currently in use.");
            return;
        }

        if (stoppingPoints == null ||
            boardingFloor < 0 ||
            boardingFloor >= stoppingPoints.Length ||
            destinationFloor < 0 ||
            destinationFloor >= stoppingPoints.Length)
        {
            Debug.LogWarning("Invalid elevator floor.");
            return;
        }

        if (stoppingPoints[boardingFloor] == null ||
            stoppingPoints[destinationFloor] == null)
        {
            Debug.LogWarning("An elevator stopping point is missing.");
            return;
        }

        if (boardingFloor == destinationFloor)
        {
            Debug.Log("Choose a different destination.");
            return;
        }

        boardingPointIndex = boardingFloor;
        destinationPointIndex = destinationFloor;

        passengers.Clear();

        if (currentPointIndex == boardingPointIndex)
        {
            state = ElevatorState.WaitingForPlayers;
            Debug.Log("Elevator is ready. Both players can board.");
        }
        else
        {
            state = ElevatorState.MovingToBoardingFloor;
            Debug.Log("Elevator is coming to the boarding floor.");
        }
    }

    private void MoveToSelectedPoint(int pointIndex)
    {
        Transform target = stoppingPoints[pointIndex];

        Vector2 oldPosition = rb.position;
        Vector2 newPosition = Vector2.MoveTowards(
            oldPosition,
            target.position,
            moveSpeed * Time.fixedDeltaTime
        );

        Vector2 movement = newPosition - oldPosition;

        rb.MovePosition(newPosition);

        foreach (Rigidbody2D passenger in passengers)
        {
            if (passenger != null)
            {
                passenger.MovePosition(passenger.position + movement);
            }
        }

        if (Vector2.Distance(newPosition, target.position) <= stoppingDistance)
        {
            Vector2 finalMovement = (Vector2)target.position - newPosition;

            rb.MovePosition(target.position);

            foreach (Rigidbody2D passenger in passengers)
            {
                if (passenger != null)
                {
                    passenger.MovePosition(
                        passenger.position + finalMovement);
                }
            }

            currentPointIndex = pointIndex;
        }
    }

    private bool ReachedPoint(int pointIndex)
    {
        return Vector2.Distance(
            rb.position,
            stoppingPoints[pointIndex].position
        ) <= stoppingDistance;
    }

    private bool HasBothPlayersBoarded()
    {
        bool player1Boarded = false;
        bool player2Boarded = false;

        foreach (Rigidbody2D passenger in passengers)
        {
            if (passenger == null) continue;

            if (passenger.CompareTag("Player1"))
                player1Boarded = true;

            if (passenger.CompareTag("Player2"))
                player2Boarded = true;
        }

        return player1Boarded && player2Boarded;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TrackPassenger(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TrackPassenger(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        Rigidbody2D player = collision.rigidbody;

        if (player != null)
            passengers.Remove(player);
    }

    private void TrackPassenger(Collision2D collision)
    {
        if (state != ElevatorState.WaitingForPlayers)
            return;

        Rigidbody2D player = collision.rigidbody;

        if (player == null) return;

        if (!player.CompareTag("Player1") &&
            !player.CompareTag("Player2"))
            return;

        if (player.transform.position.y <= transform.position.y)
            return;

        passengers.Add(player);
    }
}
