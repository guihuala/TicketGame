using UnityEngine;
using UnityEngine.UI;

public class GameOverPanel : BasePanel
{
    [Header("UI Elements")]
    [SerializeField] private Text amountText;
    [SerializeField] private Image[] stars;
    [SerializeField] private Text starRequirementText; // 显示星级要求的文本
    [SerializeField] private Button lastButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button backButton;

    private EconomyManager economyManager;
    private DaySchedule currentLevel;

    private void Start()
    {
        economyManager = FindObjectOfType<EconomyManager>();
        if (economyManager == null)
        {
            Debug.LogError("[GameOverPanel] 找不到 EconomyManager");
        }

        // 获取当前关卡
        var gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            var ticketGenerator = FindObjectOfType<TicketGenerator>();
            if (ticketGenerator != null)
            {
                currentLevel = ticketGenerator.GetCurrentDay();
            }
        }

        // 设置按钮点击事件
        lastButton.onClick.AddListener(OnLastButtonClicked);
        nextButton.onClick.AddListener(OnNextButtonClicked);
        restartButton.onClick.AddListener(OnRestartButtonClicked);
        backButton.onClick.AddListener(OnBackButtonClicked);

        // 更新显示
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (economyManager == null) return;

        // 显示当前收入
        DisplayAmount();

        // 显示星级
        DisplayStars();

        // 显示星级要求
        DisplayStarRequirements();
    }

    private void DisplayAmount()
    {
        if (amountText != null)
        {
            amountText.text = "$ " + economyManager.currentIncome.ToString();
        }
    }

    private void DisplayStars()
    {
        if (stars == null || stars.Length == 0) return;

        int starCount = economyManager.GetStarRating();

        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] != null)
            {
                // 显示获得的星星
                stars[i].enabled = (i < starCount);
                
                // 可以添加发光效果或其他视觉反馈
                if (i < starCount)
                {
                    // 获得星星的视觉效果
                    stars[i].color = Color.black;
                }
                else
                {
                    // 未获得星星的视觉效果
                    stars[i].color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
                }
            }
        }

        Debug.Log($"[GameOverPanel] 显示星级: {starCount}星");
    }

    private void DisplayStarRequirements()
    {
        if (starRequirementText != null && currentLevel != null)
        {
            string requirementText = $"评分标准:\n";
            requirementText += $"⭐ ${currentLevel.star1Income}\n";
            requirementText += $"⭐⭐ ${currentLevel.star2Income}\n";
            requirementText += $"⭐⭐⭐ ${currentLevel.star3Income}";
            
            starRequirementText.text = requirementText;
        }
    }

    // 按钮点击事件
    private void OnLastButtonClicked()
    {
        // 上一关逻辑
        Debug.Log("切换到上一关");
        // 实现关卡切换逻辑
    }

    private void OnNextButtonClicked()
    {
        // 下一关逻辑
        Debug.Log("切换到下一关");
        // 实现关卡切换逻辑
    }

    private void OnRestartButtonClicked()
    {
        GameManager.Instance.RestartCurrentLevel();
    }

    private void OnBackButtonClicked()
    {
        GameManager.Instance.ReturnToMainMenu();
    }

    // 更新关卡数据（在关卡切换时调用）
    public void UpdateLevelData(DaySchedule level)
    {
        currentLevel = level;
        UpdateDisplay();
    }
}