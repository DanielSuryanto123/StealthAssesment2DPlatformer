using UnityEngine;

/// <summary>
/// A laser or trap hazard that can be remotely switched on/off — normally by a
/// LeverController sitting in the OTHER player's area, for the cross-dependency
/// rescue mechanic.
///
/// Setup:
/// - Attach to the laser/trap GameObject.
/// - Add a Collider2D set to "Is Trigger" covering the hazard's danger zone.
/// - Make sure your Player prefabs are tagged "Player" (Unity's built-in tag)
///   so this script can detect them.
/// - Assign this component in a LeverController's "Linked Hazards" list.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class LaserHazard : MonoBehaviour
{
    [Tooltip("Whether the hazard is dangerous when the level starts, before any lever has been touched.")]
    [SerializeField] private bool startsActive = true;

    [Header("Visuals (optional)")]
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
        SetActive(startsActive);
    }

    public void SetActive(bool active)
    {
        IsActive = active;
        hazardCollider.enabled = active;

        if (laserVisual != null)
        {
            laserVisual.enabled = active;
        }
        if (laserVfx != null)
        {
            laserVfx.SetActive(active);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsActive) return;

        if (other.CompareTag("Player"))
        {
            // Hook your actual fail-state logic here, e.g.:
            // other.GetComponent<PlayerHealth>()?.Die();
            // or GameManager.Instance.RespawnPlayer(other.gameObject);
            Debug.Log($"{other.name} hit an active laser hazard.");
        }
    }
}