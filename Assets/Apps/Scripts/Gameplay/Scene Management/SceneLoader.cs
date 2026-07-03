using System;
using System.Collections;
using CoLab.UI;
using UnityEngine;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEditor;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour, IWidget
{
    public static SceneLoader Instance;
    
    // public attributes
    [SerializeField] private float transitionDuration;

    private Canvas _canvas;
    private CanvasGroup _canvasGroup;
    

    private void Awake()
    {
        if (Instance is null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        _canvas = GetComponentInChildren<Canvas>();
        _canvasGroup = GetComponentInChildren<CanvasGroup>();
    }

    private void Start()
    {
        EnableInteraction(true);
        FadeIn();
    }

    public void ChangeScene(string sceneName)
    {
        Sequence sequence = DOTween.Sequence();

        EnableInteraction(true);
        sequence.Append(FadeOut());
        sequence.AppendCallback(() =>
        {
            SceneManager.LoadSceneAsync(sceneName);
        });
    }

    private void EnableInteraction(bool value)
    {
        _canvasGroup.blocksRaycasts = !value;
    }

    [ContextMenu("Fade In")]
    public Tween FadeIn()
    {
        _canvasGroup.alpha = 1f;
        return _canvasGroup.DOFade(0f, transitionDuration);
    }

    [ContextMenu("Fade Out")]
    public Tween FadeOut()
    {
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
