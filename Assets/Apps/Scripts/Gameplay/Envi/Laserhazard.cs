
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LaserHazard : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool startsActive = true;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer laserVisual;
    [SerializeField] private GameObject laserVfx;

    private Collider2D hazardCollider;

    public bool IsActive { get; private set; }

    private void Awake()
    {
        hazardCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        ResetToStartState();
    }

    public void SetActive(bool active)
    {
        IsActive = active;

        if (hazardCollider != null)
            hazardCollider.enabled = active;

        if (laserVisual != null)
            laserVisual.enabled = active;

        if (laserVfx != null)
            laserVfx.SetActive(active);
    }

    public void ResetToStartState()
    {
        SetActive(startsActive);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsActive)
            return;

        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            other.GetComponent<PlayerRespawn>()?.Respawn();
        }
    }
}
