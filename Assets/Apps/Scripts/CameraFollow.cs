using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player1;
    public Transform player2;

    public float smoothSpeed = 5f;

    private Vector3 offset;

    void Start()
    {
        offset = transform.position;
    }

    void LateUpdate()
    {
        Vector3 middlePoint = (player1.position + player2.position) / 2f;

        Vector3 targetPosition = new Vector3(
            middlePoint.x,
            middlePoint.y,
            offset.z
        );

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );
    }
}