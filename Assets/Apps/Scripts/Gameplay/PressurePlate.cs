using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public GameObject bridge;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player1") || collision.CompareTag("Player2"))
        {
            bridge.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player1") || collision.CompareTag("Player2"))
        {
            bridge.SetActive(false);
        }
    }
}