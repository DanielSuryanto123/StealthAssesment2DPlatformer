using UnityEngine;
using UnityEngine.SceneManagement;

public class EndPortal : MonoBehaviour
{
    private bool player1Inside;
    private bool player2Inside;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player1"))
            player1Inside = true;

        if (other.CompareTag("Player2"))
            player2Inside = true;

        CheckPortal();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player1"))
            player1Inside = false;

        if (other.CompareTag("Player2"))
            player2Inside = false;
    }

    private void CheckPortal()
    {
        if (player1Inside && player2Inside)
        {
            int currentScene = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentScene + 1);
        }
    }
}