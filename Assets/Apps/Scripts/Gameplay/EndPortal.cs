using UnityEngine;
using UnityEngine.SceneManagement;

public class EndPortal : MonoBehaviour
{
    private bool redInside;
    private bool blueInside;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("RedPlayer"))
        {
            redInside = true;
        }

        if(other.CompareTag("BluePlayer"))
        {
            blueInside = true;
        }

        CheckPortal();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.CompareTag("RedPlayer"))
        {
            redInside = false;
        }

        if(other.CompareTag("BluePlayer"))
        {
            blueInside = false;
        }
    }

    private void CheckPortal()
    {
        if(redInside && blueInside)
        {
            LoadNextLevel();
        }
    }

    private void LoadNextLevel()
    {
        int currentScene =
            SceneManager.GetActiveScene().buildIndex;

        SceneManager.LoadScene(currentScene + 1);
    }
}