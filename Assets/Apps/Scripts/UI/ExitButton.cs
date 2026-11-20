using UnityEngine;

public class ExitButton : MonoBehaviour
{
    public void ExitGame()
    {
        Debug.Log("Player exited the game.");
        Application.Quit();
    }
}
