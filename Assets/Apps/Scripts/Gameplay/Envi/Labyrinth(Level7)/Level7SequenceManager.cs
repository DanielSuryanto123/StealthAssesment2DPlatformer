using System.Collections.Generic;
using UnityEngine;

public class Level7SequenceManager : MonoBehaviour
{
    [Header("Puzzle Settings")]
    [SerializeField] private int totalButtons = 3;
    [SerializeField] private GameObject exitDoor;

    private HashSet<int> activatedButtons = new HashSet<int>();

    public int ActivatedButtonCount => activatedButtons.Count;
    public bool IsComplete => activatedButtons.Count >= totalButtons;

    private void Start()
    {
        if (exitDoor != null)
            exitDoor.SetActive(true);

        EnsurePlayerInteraction("Player1");
        EnsurePlayerInteraction("Player2");
    }

    private static void EnsurePlayerInteraction(string playerTag)
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);

        if (player == null)
        {
            Debug.LogWarning("Level7SequenceManager: no object tagged " + playerTag + " found.");
            return;
        }

        if (player.GetComponent<Level7PlayerInteraction>() == null)
            player.AddComponent<Level7PlayerInteraction>();
    }

    public void RegisterButton(int buttonID)
    {
        if (IsComplete || !activatedButtons.Add(buttonID))
            return;

        Debug.Log("Button " + buttonID + " activated. " +
                  ActivatedButtonCount + "/" + totalButtons);

        if (IsComplete)
        {
            if (exitDoor != null)
                exitDoor.SetActive(false);

            Debug.Log("All buttons activated. Exit unlocked!");
        }
    }
}