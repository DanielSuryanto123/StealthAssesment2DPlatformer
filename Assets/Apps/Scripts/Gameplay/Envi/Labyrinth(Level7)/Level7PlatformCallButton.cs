using UnityEngine;

public class Level7PlatformCallButton : MonoBehaviour
{
    [Header("Platform")]
    [SerializeField] private Level7MovingPlatform targetPlatform;

    [Header("Floor Selection")]
    [SerializeField] private int boardingFloorIndex = 0;
    [SerializeField] private int destinationIndex = 1;

    public void Interact(GameObject actor)
    {
        if (actor == null) return;

        if (!actor.CompareTag("Player1") &&
            !actor.CompareTag("Player2"))
            return;

        if (targetPlatform == null)
        {
            Debug.LogWarning("Elevator platform is not assigned.");
            return;
        }

        targetPlatform.RequestTrip(
            boardingFloorIndex,
            destinationIndex
        );

        Debug.Log(
            actor.name + " requested floor " + destinationIndex);
    }
}
