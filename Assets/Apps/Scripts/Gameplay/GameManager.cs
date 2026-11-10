using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int totalCoins;

    private int collectedCoins = 0;

    public GameObject door;

    void Awake()
    {
        instance = this;
    }

    public void CollectCoin()
    {
        collectedCoins++;

        Debug.Log("Collected Coins: " + collectedCoins);

        if (collectedCoins >= totalCoins)
        {
            Debug.Log("ALL COINS COLLECTED");

            door.SetActive(false);
        }
    }
}