using UnityEngine;
using TMPro;
using System;

public class GlobalGameTimer : MonoBehaviour
{
    public static GlobalGameTimer Instance { get; private set; }

    [Header("Timer Settings")]
    [SerializeField] private float totalTime = 900f;
    [SerializeField] private TMP_Text timerText;

    private float remainingTime;
    private float levelTimeCheckpoint;

    private bool timerRunning;
    private bool timeExpired;

    public float RemainingTime => remainingTime;
    public bool IsRunning => timerRunning;
    public bool IsExpired => timeExpired;

    public event Action OnTimeExpired;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        remainingTime = totalTime;
        levelTimeCheckpoint = totalTime;

        UpdateTimerDisplay();
    }

    private void Update()
    {
        if (!timerRunning || timeExpired)
            return;

        remainingTime -= Time.unscaledDeltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            timerRunning = false;
            timeExpired = true;

            UpdateTimerDisplay();
            Debug.Log("GLOBAL TIMER EXPIRED!");

            OnTimeExpired?.Invoke();
            return;
        }

        UpdateTimerDisplay();
    }

    public void StartTimer()
    {
        if (timeExpired) return;

        timerRunning = true;
    }

    public void StopTimer()
    {
        timerRunning = false;
    }

    // Save the remaining time when entering a level.
    public void SaveLevelCheckpoint()
    {
        levelTimeCheckpoint = remainingTime;

        Debug.Log("Level checkpoint saved: " + levelTimeCheckpoint);
    }

    // Restore the saved time when restarting the current level.
    public void RestoreLevelCheckpoint()
    {
        remainingTime = levelTimeCheckpoint;
        timeExpired = false;
        timerRunning = true;

        UpdateTimerDisplay();

        Debug.Log("Level checkpoint restored: " + remainingTime);
    }

    public void UpdateTimerDisplay()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);

        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}
