
using UnityEngine;

public class SinglePlayerDoorController : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private GameObject doorVisual;
    [SerializeField] private Collider2D doorCollider;

    private bool isOpen;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        SetDoorState(false);
    }

    public void OpenDoor()
    {
        SetDoorState(true);
    }

    public void CloseDoor()
    {
        SetDoorState(false);
    }

    public void ToggleDoor()
    {
        SetDoorState(!isOpen);
    }

    private void SetDoorState(bool open)
    {
        isOpen = open;

        if (doorVisual != null)
            doorVisual.SetActive(!open);

        if (doorCollider != null)
            doorCollider.enabled = !open;

        Debug.Log(open ? "Door opened!" : "Door closed!");
    }
}
