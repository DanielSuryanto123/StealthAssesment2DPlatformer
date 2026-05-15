using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int totalCoins;
    private int collectedCoins;

    public GameObject door;

    void Awake()
    {
        instance = this;
    }

    public void CollectCoin()
    {
        collectedCoins++;

        Debug.Log("Coins: " + collectedCoins + "/" + totalCoins);

        if (collectedCoins >= totalCoins)
        {
            door.SetActive(false);

            Debug.Log("Door Opened!");
        }
    }
}