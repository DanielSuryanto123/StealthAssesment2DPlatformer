using UnityEngine;

public class Coin : MonoBehaviour
{
    public bool player1Only;
    public bool player2Only;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // PLAYER 1
        if (collision.CompareTag("Player1"))
        {
            if (player2Only)
                return;

            Destroy(gameObject);
        }

        // PLAYER 2
        if (collision.CompareTag("Player2"))
        {
            if (player1Only)
                return;

            Destroy(gameObject);
        }
    }
}