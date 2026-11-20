
using System;
using CoLab.UI;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour, IWidget
{
    public static SceneLoader Instance;

    [Header("Scene Settings")]
    [SerializeField] private string firstLevelSceneName = "Level1";

    [Header("Transition Settings")]
    [SerializeField] private float transitionDuration = 0.5f;

    private Canvas _canvas;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _canvas = GetComponentInChildren<Canvas>(true);
        _canvasGroup = GetComponentInChildren<CanvasGroup>(true);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        // Start with the transition screen invisible.
        _canvasGroup.alpha = 1f;
        EnableInteraction(true);
        FadeIn();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Start the global timer only when Level 1 loads.
        if (scene.name == firstLevelSceneName)
        {
            if (GlobalGameTimer.Instance != null)
            {
                GlobalGameTimer.Instance.StartTimer();
                Debug.Log("Global timer started!");
            }
            else
            {
                Debug.LogError("GlobalGameTimer instance not found!");
            }
        }

        // Fade in after the new scene has loaded.
        EnableInteraction(true);
        FadeIn();
    }

    public void ChangeScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene name is empty!");
            return;
        }

        // Prevent multiple scene transitions.
        if (DOTween.IsTweening(_canvasGroup))
            return;

        EnableInteraction(false);

        Sequence sequence = DOTween.Sequence();

        sequence.Append(FadeOut());

        sequence.AppendCallback(() =>
        {
            SceneManager.LoadSceneAsync(sceneName);
        });
    }

    private void EnableInteraction(bool value)
    {
        _canvasGroup.blocksRaycasts = !value;
        _canvasGroup.interactable = !value;
    }

    public Tween FadeIn()
    {
        _canvasGroup.DOKill();
        _canvasGroup.alpha = 1f;

        return _canvasGroup.DOFade(0f, transitionDuration)
            .OnComplete(() => EnableInteraction(true));
    }

    public Tween FadeOut()
    {
        _canvasGroup.DOKill();
        _canvasGroup.alpha = 0f;

        return _canvasGroup.DOFade(1f, transitionDuration);
    }

    public void Show()
    {
        _canvas.enabled = true;
    }

    public void Hide()
    {
        _canvas.enabled = false;
    }
}
