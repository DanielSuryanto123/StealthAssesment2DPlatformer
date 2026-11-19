
using UnityEngine;
using Cinemachine;

public class CameraModeController : MonoBehaviour
{
    public enum CameraMode
    {
        Normal,
        BlindSync
    }

    [Header("Cinemachine Settings")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    [Header("Camera Zoom Settings")]
    [SerializeField] private float normalZoom = 10f;
    [SerializeField] private float blindSyncZoom = 5.5f;

    [Header("Transition Settings")]
    [SerializeField] private float zoomSmoothness = 3f;

    private float targetZoom;
    private float zoomVelocity;

    public CameraMode CurrentMode { get; private set; }

    private void Awake()
    {
        if (virtualCamera == null)
        {
            Debug.LogError("CameraModeController: Virtual Camera is not assigned!");
            enabled = false;
            return;
        }

        // Start with normal gameplay camera
        CurrentMode = CameraMode.Normal;
        targetZoom = normalZoom;

        virtualCamera.m_Lens.OrthographicSize = normalZoom;
    }

    private void Update()
    {
        float currentZoom = virtualCamera.m_Lens.OrthographicSize;

        float newZoom = Mathf.SmoothDamp(
            currentZoom,
            targetZoom,
            ref zoomVelocity,
            1f / Mathf.Max(zoomSmoothness, 0.01f),
            Mathf.Infinity,
            Time.unscaledDeltaTime
        );

        virtualCamera.m_Lens.OrthographicSize = newZoom;
    }

    public void SetNormalMode()
    {
        CurrentMode = CameraMode.Normal;
        targetZoom = normalZoom;
        zoomVelocity = 0f;

        Debug.Log("Camera switched to NORMAL mode.");
    }

    public void SetBlindSyncMode()
    {
        CurrentMode = CameraMode.BlindSync;
        targetZoom = blindSyncZoom;
        zoomVelocity = 0f;

        Debug.Log("Camera switched to BLIND SYNC mode.");
    }

    public void SetCameraMode(CameraMode mode)
    {
        if (mode == CameraMode.Normal)
            SetNormalMode();
        else
            SetBlindSyncMode();
    }
}
