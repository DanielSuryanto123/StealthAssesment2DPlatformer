using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Level6MovingPlatform : MonoBehaviour
{
    [Header("Platform Stops")]
    [SerializeField] private Transform startingPoint;
    [SerializeField] private Transform destinationPoint;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stoppingDistance = 0.05f;

    private Rigidbody2D rb;
    private bool isMoving;
    private bool hasMoved;

    private readonly HashSet<Rigidbody2D> passengers =
        new HashSet<Rigidbody2D>();

    public bool IsMoving => isMoving;
    public bool HasMoved => hasMoved;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        if (startingPoint == null || destinationPoint == null)
        {
            Debug.LogError("Level6MovingPlatform: Assign both platform points.");
            enabled = false;
            return;
        }

        rb.position = startingPoint.position;
    }

    private void FixedUpdate()
    {
        passengers.RemoveWhere(player => player == null);

        if (!isMoving)
            return;

        Vector2 oldPosition = rb.position;

        Vector2 newPosition = Vector2.MoveTowards(
            oldPosition,
            destinationPoint.position,
            moveSpeed * Time.fixedDeltaTime
        );

        Vector2 movement = newPosition - oldPosition;

        rb.MovePosition(newPosition);

        // Move every player standing on the platform by the same distance.
        foreach (Rigidbody2D passenger in passengers)
        {
            if (passenger != null)
            {
                passenger.MovePosition(passenger.position + movement);
            }
        }

        if (Vector2.Distance(newPosition, destinationPoint.position)
            <= stoppingDistance)
        {
            Vector2 finalMovement =
                (Vector2)destinationPoint.position - newPosition;

            rb.MovePosition(destinationPoint.position);

            foreach (Rigidbody2D passenger in passengers)
            {
                if (passenger != null)
                {
                    passenger.MovePosition(
                        passenger.position + finalMovement
                    );
                }
            }

            isMoving = false;
            hasMoved = true;

            Debug.Log("Level 6 platform reached its destination.");
        }
    }

    public void RequestTrip(int boardingFloor, int destinationFloor)
    {
        if (isMoving || hasMoved)
            return;

        isMoving = true;
        Debug.Log("Level 6 platform moving upward.");
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
        Rigidbody2D player = collision.rigidbody;

        if (player == null)
            return;

        if (!player.CompareTag("Player1") &&
            !player.CompareTag("Player2"))
            return;

        // Only track players standing above the platform.
        if (player.position.y <= transform.position.y)
            return;

        passengers.Add(player);
    }
}
