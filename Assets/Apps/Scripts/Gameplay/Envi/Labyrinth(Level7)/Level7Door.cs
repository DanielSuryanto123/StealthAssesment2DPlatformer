using UnityEngine;

public class Level7Door : MonoBehaviour
{
    [Header("Door Components")]
    [SerializeField] private Collider2D doorCollider;
    [SerializeField] private GameObject closedVisual;
    [SerializeField] private GameObject openVisual;

    private bool isOpen;

    public bool IsOpen => isOpen;

    public void OpenDoor()
    {
        if (isOpen)
            return;

        isOpen = true;

        if (doorCollider != null)
            doorCollider.enabled = false;

        if (closedVisual != null)
            closedVisual.SetActive(false);

        if (openVisual != null)
            openVisual.SetActive(true);

        Debug.Log(gameObject.name + " opened.");
    }
}