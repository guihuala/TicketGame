using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameOverPanel : BasePanel
{
    [Header("UI Elements")]
    [SerializeField] private Text amountText;  // 显示金额的 Text
    [SerializeField] private Image[] stars;    // 显示星级的图标
    [SerializeField] private Button lastButton; // "Last" 按钮
    [SerializeField] private Button nextButton; // "Next" 按钮
    [SerializeField] private Button restartButton;
    [SerializeField] private Button backButton;

    private int totalAmount = 60;  // 示例金额
    private int starCount = 3;  // 示例星级

    private void Start()
    {
        // 初始化 UI
        DisplayAmount();
        DisplayStars();

        // 设置按钮点击事件
        lastButton.onClick.AddListener(OnLastButtonClicked);
        nextButton.onClick.AddListener(OnNextButtonClicked);
        restartButton.onClick.AddListener(OnRestartButtonClicked);
        backButton.onClick.AddListener(OnBackButtonClicked);
    }

    private void DisplayAmount()
    {
        amountText.text = "$ " + totalAmount.ToString();  // 显示金额
    }

    private void DisplayStars()
    {
        // 根据星级数量显示星星图标
        for (int i = 0; i < stars.Length; i++)
        {
            if (i < starCount)
                stars[i].enabled = true;  // 显示星星
            else
                stars[i].enabled = false;  // 隐藏星星
        }
    }

    // 按钮点击事件
    private void OnLastButtonClicked()
    {
        
    }

    private void OnNextButtonClicked()
    {
        
    }

    private void OnRestartButtonClicked()
    {
        GameManager.Instance.RestartCurrentLevel();
    }

    private void OnBackButtonClicked()
    {
        GameManager.Instance.ReturnToMainMenu();
    }
}