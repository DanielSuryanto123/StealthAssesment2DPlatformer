using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class RescuePlatform : MonoBehaviour
{
    [Header("Platform Movement")]
    [SerializeField] private Transform upperPosition;
    [SerializeField] private float moveSpeed = 2f;

    private Rigidbody2D platformRb;
    private bool isMoving;

    public bool IsReady => upperPosition != null && platformRb != null;
    public bool IsMoving => isMoving;

    private void Awake()
    {
        platformRb = GetComponent<Rigidbody2D>();

        platformRb.bodyType = RigidbodyType2D.Kinematic;
        platformRb.gravityScale = 0f;
    }

    public bool StartRescue(Transform player)
    {
        if (isMoving)
        {
            Debug.LogWarning("RescuePlatform: Platform is already moving.");
            return false;
        }

        if (player == null || !IsReady)
        {
            Debug.LogError(
                "RescuePlatform: Player or Upper Position is missing."
            );
            return false;
        }

        StartCoroutine(MovePlatform(player));
        return true;
    }

    private IEnumerator MovePlatform(Transform player)
    {
        isMoving = true;

        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();

        while (Vector2.Distance(
            platformRb.position,
            upperPosition.position
        ) > 0.02f)
        {
            if (player == null)
                break;

            Vector2 nextPosition = Vector2.MoveTowards(
                platformRb.position,
                upperPosition.position,
                moveSpeed * Time.fixedDeltaTime
            );

            Vector2 movement = nextPosition - platformRb.position;

            platformRb.MovePosition(nextPosition);

            if (playerRb != null)
            {
                playerRb.MovePosition(playerRb.position + movement);
            }
            else
            {
                player.position += (Vector3)movement;
            }

            yield return new WaitForFixedUpdate();
        }

        if (player != null)
        {
            platformRb.MovePosition(upperPosition.position);
        }

        isMoving = false;

        Debug.Log("RescuePlatform: Reached upper position.");
    }
}
