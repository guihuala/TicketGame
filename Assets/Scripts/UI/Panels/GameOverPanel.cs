using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameOverPanel : BasePanel
{
    [Header("UI Elements")]
    [SerializeField] private Text amountText;
    [SerializeField] private Image[] stars;
    [SerializeField] private Button lastButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button backButton;

    private EconomyManager economyManager;
    private TicketGenerator ticketGenerator;
    private int currentLevelIndex;
    private bool isStarAnimationPlaying = false;

    private void Start()
    {
        economyManager = FindObjectOfType<EconomyManager>();
        ticketGenerator = FindObjectOfType<TicketGenerator>();
        
        if (economyManager == null)
        {
            Debug.LogError("[GameOverPanel] 找不到 EconomyManager");
        }

        // 获取当前关卡索引
        currentLevelIndex = PlayerPrefs.GetInt("SelectedLevelIndex", 0);
        
        // 设置按钮点击事件
        lastButton.onClick.AddListener(OnLastButtonClicked);
        nextButton.onClick.AddListener(OnNextButtonClicked);
        restartButton.onClick.AddListener(OnRestartButtonClicked);
        backButton.onClick.AddListener(OnBackButtonClicked);

        // 初始隐藏所有星星
        InitializeStars();
        
        // 更新按钮显示状态
        UpdateButtonStates();
        
        // 更新显示
        UpdateDisplay();
    }

    private void InitializeStars()
    {
        if (stars == null || stars.Length == 0) return;

        // 初始隐藏所有星星
        foreach (var star in stars)
        {
            if (star != null)
            {
                star.enabled = false;
                // 确保星星初始缩放正常
                star.transform.localScale = Vector3.one;
            }
        }
    }

    private void UpdateDisplay()
    {
        if (economyManager == null) return;

        // 显示当前收入
        DisplayAmount();

        // 开始星星动画
        StartStarAnimation();
    }

    private void DisplayAmount()
    {
        if (amountText != null)
        {
            amountText.text = "$ " + economyManager.currentIncome.ToString();
        }
    }

    /// <summary>
    /// 开始星星显示动画
    /// </summary>
    private void StartStarAnimation()
    {
        if (isStarAnimationPlaying) return;
        
        int starCount = economyManager.GetStarRating();
        Debug.Log($"[GameOverPanel] 开始星星动画，获得 {starCount} 颗星");
        
        StartCoroutine(StarAnimationCoroutine(starCount));
    }

    /// <summary>
    /// 星星动画协程
    /// </summary>
    private IEnumerator StarAnimationCoroutine(int starCount)
    {
        isStarAnimationPlaying = true;
        
        // 等待一帧确保UI已经初始化
        yield return null;
        
        for (int i = 0; i < starCount; i++)
        {
            if (i < stars.Length && stars[i] != null)
            {
                // 显示当前星星
                stars[i].enabled = true;
                
                // 根据星星数量播放对应的音效
                PlayStarSoundEffect(i + 1);
                
                Debug.Log($"[GameOverPanel] 显示第 {i + 1} 颗星");

                // 播放缩放动画并等待完成
                yield return StartCoroutine(PlayStarScaleAnimation(stars[i].transform));
            }
            
            // 如果不是最后一颗星，等待1秒显示下一颗星
            if (i < starCount - 1)
            {
                yield return new WaitForSeconds(1f);
            }
        }
        
        isStarAnimationPlaying = false;
        Debug.Log($"[GameOverPanel] 星星动画完成");
    }

    /// <summary>
    /// 播放星星缩放动画
    /// </summary>
    private IEnumerator PlayStarScaleAnimation(Transform starTransform)
    {
        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = originalScale * 1.5f; // 稍微增大缩放倍数
        
        // 放大
        float duration = 0.3f;
        float timer = 0f;
        
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = timer / duration;
            starTransform.localScale = Vector3.Lerp(originalScale, targetScale, progress);
            yield return null;
        }
        
        starTransform.localScale = targetScale;
        
        // 在最大缩放位置短暂停留
        yield return new WaitForSeconds(0.1f);
        
        // 缩小回原尺寸
        timer = 0f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = timer / duration;
            starTransform.localScale = Vector3.Lerp(targetScale, originalScale, progress);
            yield return null;
        }
        
        starTransform.localScale = originalScale;
    }

    /// <summary>
    /// 根据星星数量播放对应的音效
    /// </summary>
    private void PlayStarSoundEffect(int starNumber)
    {
        string soundName = "";
        
        switch (starNumber)
        {
            case 1:
                soundName = "1Star";
                break;
            case 2:
                soundName = "2Star";
                break;
            case 3:
                soundName = "3Star";
                break;
            default:
                soundName = "1Star"; // 默认音效
                break;
        }
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfx(soundName);
            Debug.Log($"[GameOverPanel] 播放音效: {soundName}");
        }
        else
        {
            Debug.LogWarning($"[GameOverPanel] AudioManager 不可用，无法播放音效: {soundName}");
        }
    }
    
    /// <summary>
    /// 更新按钮显示状态
    /// </summary>
    private void UpdateButtonStates()
    {
        // 检查是否有上一关
        bool hasLastLevel = currentLevelIndex > 0;
        lastButton.gameObject.SetActive(hasLastLevel);

        // 检查是否有下一关
        bool hasNextLevel = IsNextLevelAvailable();
        nextButton.gameObject.SetActive(hasNextLevel);

        Debug.Log($"[GameOverPanel] 当前关卡: {currentLevelIndex}, 有上一关: {hasLastLevel}, 有下一关: {hasNextLevel}");
    }

    /// <summary>
    /// 检查下一关是否可用（已解锁且存在）
    /// </summary>
    private bool IsNextLevelAvailable()
    {
        // 检查关卡是否已解锁
        bool isUnlocked = LevelProgressManager.IsLevelUnlocked(currentLevelIndex + 1);
        
        // 检查关卡数据是否存在
        bool levelExists = CheckLevelExists(currentLevelIndex + 1);
        
        return isUnlocked && levelExists;
    }

    /// <summary>
    /// 检查指定索引的关卡是否存在
    /// </summary>
    private bool CheckLevelExists(int levelIndex)
    {
        if (ticketGenerator == null) return false;
        
        // 临时设置关卡索引来检查是否存在
        int originalIndex = ticketGenerator.GetCurrentDay() != null ? currentLevelIndex : -1;
        ticketGenerator.SetLevel(levelIndex);
        bool exists = ticketGenerator.GetCurrentDay() != null;
        
        // 恢复原始索引
        if (originalIndex >= 0)
        {
            ticketGenerator.SetLevel(originalIndex);
        }
        
        return exists;
    }

    // 按钮点击事件
    private void OnLastButtonClicked()
    {
        // 切换到上一关
        int lastLevelIndex = currentLevelIndex - 1;
        if (lastLevelIndex >= 0)
        {
            LoadLevel(lastLevelIndex);
        }
        else
        {
            Debug.LogWarning("[GameOverPanel] 已经是第一关，没有上一关");
        }
    }

    private void OnNextButtonClicked()
    {
        // 切换到下一关
        int nextLevelIndex = currentLevelIndex + 1;
        if (IsNextLevelAvailable())
        {
            LoadLevel(nextLevelIndex);
        }
        else
        {
            Debug.LogWarning("[GameOverPanel] 下一关不可用或不存在");
        }
    }

    private void OnRestartButtonClicked()
    {
        GameManager.Instance.RestartCurrentLevel();
    }

    private void OnBackButtonClicked()
    {
        GameManager.Instance.ReturnToMainMenu();
    }

    /// <summary>
    /// 加载指定关卡
    /// </summary>
    private void LoadLevel(int levelIndex)
    {
        Debug.Log($"[GameOverPanel] 加载关卡: {levelIndex}");
        
        // 保存选中的关卡索引
        PlayerPrefs.SetInt("SelectedLevelIndex", levelIndex);
        PlayerPrefs.Save();
        
        // 重新加载游戏场景
        GameManager.Instance.RestartCurrentLevel();
    }

    // 当面板被禁用时停止所有协程
    private void OnDisable()
    {
        StopAllCoroutines();
        isStarAnimationPlaying = false;
    }
}