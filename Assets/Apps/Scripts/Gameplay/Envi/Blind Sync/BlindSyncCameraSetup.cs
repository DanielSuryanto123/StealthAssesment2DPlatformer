
using UnityEngine;

public class BlindSyncCameraSetup : MonoBehaviour
{
    [Header("Players")]
    [SerializeField] private Transform player1;
    [SerializeField] private Transform player2;

    [Header("Cameras")]
    [SerializeField] private Camera camera1;
    [SerializeField] private Camera camera2;

    [Header("Camera Settings")]
    [SerializeField] private float cameraHeight = 5f;
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float cameraZ = -10f;

    private void Start()
    {
        if (camera1 == null || camera2 == null)
        {
            Debug.LogError("Assign both cameras in BlindSyncCameraSetup.");
            enabled = false;
            return;
        }

        camera1.rect = new Rect(0f, 0f, 0.5f, 1f);
        camera2.rect = new Rect(0.5f, 0f, 0.5f, 1f);

        camera1.aspect = 0.5f * Screen.width / Screen.height;
        camera2.aspect = 0.5f * Screen.width / Screen.height;

        camera1.orthographic = true;
        camera2.orthographic = true;

        camera1.orthographicSize = cameraHeight;
        camera2.orthographicSize = cameraHeight;

        camera1.cullingMask = LayerMask.GetMask("RedRoom");
        camera2.cullingMask = LayerMask.GetMask("BlueRoom");
    }

    private void LateUpdate()
    {
        FollowPlayer(camera1, player1);
        FollowPlayer(camera2, player2);
    }

    private void FollowPlayer(Camera cam, Transform target)
    {
        if (cam == null || target == null)
            return;

        Vector3 targetPosition = new Vector3(
            target.position.x,
            target.position.y,
            cameraZ
        );

        cam.transform.position = Vector3.Lerp(
            cam.transform.position,
            targetPosition,
            followSpeed * Time.deltaTime
        );
    }
}
