using UnityEngine;

/// <summary>
/// The visual "?" icon spawned by PlayerController's ping. Pops in, hovers,
/// then destroys itself after a short lifetime. Optionally follows the
/// player so it doesn't hang in empty air if they keep moving.
///
/// Setup:
/// - Attach this to your ping icon prefab (a world-space Canvas + Image,
///   or a SpriteRenderer with a "?" sprite).
/// </summary>
public class PingIndicator : MonoBehaviour
{
    [SerializeField] private float lifetime = 1.5f;
    [SerializeField] private float popScale = 1.2f;
    [SerializeField] private float popDuration = 0.15f;
    [SerializeField] private float bobHeight = 0.15f;
    [SerializeField] private float bobSpeed = 3f;
    [SerializeField] private Vector3 followOffset = new Vector3(0, 0.5f, 0);

    private Transform followTarget;
    private float timer;
    private Vector3 baseScale;

    private void Awake()
    {
        baseScale = transform.localScale;
        transform.localScale = Vector3.zero;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer <= popDuration)
        {
            float t = timer / popDuration;
            transform.localScale = Vector3.Lerp(Vector3.zero, baseScale * popScale, t);
        }
        else
        {
            float settleT = Mathf.Clamp01((timer - popDuration) / 0.1f);
            transform.localScale = Vector3.Lerp(baseScale * popScale, baseScale, settleT);
        }

        float bob = Mathf.Sin(timer * bobSpeed) * bobHeight;

        if (followTarget != null)
        {
            transform.position = followTarget.position + followOffset + Vector3.up * bob;
        }
        else
        {
            transform.position += Vector3.up * bob * Time.deltaTime;
        }

        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    public void FollowTarget(Transform target)
    {
        followTarget = target;
    }
}