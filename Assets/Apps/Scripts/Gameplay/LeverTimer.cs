using UnityEngine;
using System;

/// <summary>
/// A single level-wide countdown timer, acting as the only "penalty" —
/// there's no lives system, so time lost from respawning is the cost of
/// hitting a hazard. This timer does NOT pause or reset on respawn.
///
/// Setup:
/// - Put this on a persistent "Managers" GameObject, alongside CheckpointManager.
/// - Subscribe UI (e.g. a text countdown) to TimeChanged.
/// - Subscribe fail-state logic (e.g. level fail screen) to TimeExpired.
/// </summary>
public class LevelTimer : MonoBehaviour
{
    [SerializeField] private float durationSeconds = 15f * 60f;

    private float remaining;
    private bool isRunning = true;

    public float RemainingSeconds => remaining;
    public event Action<float> TimeChanged;
    public event Action TimeExpired;

    private void Start()
    {
        remaining = durationSeconds;
        TimeChanged?.Invoke(remaining);
    }

    private void Update()
    {
        if (!isRunning) return;

        remaining -= Time.deltaTime;

        if (remaining <= 0f)
        {
            remaining = 0f;
            isRunning = false;
            TimeChanged?.Invoke(remaining);
            TimeExpired?.Invoke();
            return;
        }

        TimeChanged?.Invoke(remaining);
    }

    public void StopTimer() => isRunning = false;
    public void ResumeTimer() => isRunning = true;
}