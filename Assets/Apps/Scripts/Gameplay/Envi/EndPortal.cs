using UnityEngine;
using UnityEngine.SceneManagement;

public class EndPortal : MonoBehaviour
{
    private bool player1Inside;
    private bool player2Inside;
    private bool isLoading;

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
        if (player1Inside && player2Inside && !isLoading)
        {
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                isLoading = true;
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                Debug.Log("All stages completed!");
            }
        }
    }
}