using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TitleUIController : MonoBehaviour
{
    public Button startButton;
    public Button settingsButton;
    public Button exitButton;
    
    [Header("入场动画设置")]
    [SerializeField] private Transform targetTransform; // 要移动的UI容器
    [SerializeField] private Vector2 startPosition;     // 起始位置（屏幕右侧外）
    [SerializeField] private float moveDuration = 1f;   // 移动持续时间
    [SerializeField] private Ease moveEase = Ease.OutBack; // 缓动类型

    private void Awake()
    {
        startButton.onClick.AddListener(OnStartButtonClicked);
        settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        exitButton.onClick.AddListener(OnExitButtonClicked);
    }

    private void Start()
    {
        PlayEntranceAnimation();
    }

    /// <summary>
    /// 播放UI入场动画
    /// </summary>
    private void PlayEntranceAnimation()
    {
        if (targetTransform == null)
        {
            Debug.LogWarning("TitleUIController: 未指定目标Transform，使用自身Transform");
            targetTransform = transform;
        }

        // 保存原始位置
        Vector3 originalPosition = targetTransform.position;
        
        // 设置起始位置（屏幕右侧外）
        if (startPosition != Vector2.zero)
        {
            // 使用指定的起始位置
            targetTransform.position = new Vector3(startPosition.x, startPosition.y, originalPosition.z);
        }
        else
        {
            // 默认从屏幕右侧外进入
            RectTransform rectTransform = targetTransform as RectTransform;
            if (rectTransform != null)
            {
                // 对于UI元素，使用anchoredPosition
                float screenWidth = Screen.width;
                rectTransform.anchoredPosition = new Vector2(screenWidth, rectTransform.anchoredPosition.y);
            }
            else
            {
                // 对于普通Transform
                targetTransform.position = new Vector3(Screen.width, originalPosition.y, originalPosition.z);
            }
        }

        // 执行移动动画
        targetTransform.DOMove(originalPosition, moveDuration)
            .SetEase(moveEase)
            .OnComplete(() =>
            {
                Debug.Log("Title UI 入场动画完成");
            });
    }

    public void OnStartButtonClicked()
    {
        SceneLoader.Instance.LoadScene(GameScene.LevelSelection);
    }

    public void OnSettingsButtonClicked()
    {
        UIManager.Instance.OpenPanel("SettingPanel");
    }

    public void OnExitButtonClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// 重置UI位置（用于重新播放动画）
    /// </summary>
    public void ResetPosition()
    {
        if (targetTransform != null)
        {
            Vector3 originalPosition = targetTransform.position;
            if (startPosition != Vector2.zero)
            {
                targetTransform.position = new Vector3(startPosition.x, startPosition.y, originalPosition.z);
            }
            else
            {
                RectTransform rectTransform = targetTransform as RectTransform;
                if (rectTransform != null)
                {
                    float screenWidth = Screen.width;
                    rectTransform.anchoredPosition = new Vector2(screenWidth, rectTransform.anchoredPosition.y);
                }
                else
                {
                    targetTransform.position = new Vector3(Screen.width, originalPosition.y, originalPosition.z);
                }
            }
        }
    }
}