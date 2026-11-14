
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
    private bool timerRunning;
    private bool timeExpired;

    public float RemainingTime => remainingTime;
    public bool IsRunning => timerRunning;
    public bool IsExpired => timeExpired;

    public event Action OnTimeExpired;

    private void Awake()
    {
        // Prevent duplicate timers
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Keep timer and its Canvas across scenes
        DontDestroyOnLoad(gameObject);

        remainingTime = totalTime;
        UpdateTimerDisplay();
    }

    private void Start()
    {
        // Start automatically when the game begins
        StartTimer();
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
        if (timeExpired)
            return;

        timerRunning = true;
    }

    public void StopTimer()
    {
        timerRunning = false;
    }

    public void UpdateTimerDisplay()
    {
        if (timerText == null)
            return;

        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);

        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}
