using System;
using UnityEngine;
using UnityEngine.UI;

public class HudController : MonoBehaviour
{
    public Button pauseButton;  // 暂停按钮
    public Text moneyText;      // 显示金钱的 Text
    public Text timeText;       // 显示时间的 Text

    private EconomyManager economyManager;
    private ScheduleClock scheduleClock;

    private void Awake()
    {
        // 绑定按钮点击事件
        pauseButton.onClick.AddListener(OnPauseButtonClicked);

        // 获取相关管理器
        economyManager = FindObjectOfType<EconomyManager>();
        scheduleClock = FindObjectOfType<ScheduleClock>();
    }

    private void Update()
    {
        // 更新 HUD 上的金钱信息
        UpdateMoneyDisplay();
        
        // 更新 HUD 上的时间信息
        UpdateTimeDisplay();
    }

    private void UpdateMoneyDisplay()
    {
        if (economyManager != null && moneyText != null)
        {
            moneyText.text = "Money: $" + economyManager.currentIncome.ToString();  // 显示当前金钱
        }
    }

    private void UpdateTimeDisplay()
    {
        if (scheduleClock != null && timeText != null)
        {
            // 将模拟时间转换为分钟：秒格式
            TimeSpan time = TimeSpan.FromSeconds(scheduleClock.simSeconds);
            timeText.text = "Time: " + time.ToString(@"hh\:mm\:ss");  // 格式为小时:分钟:秒
        }
    }

    private void OnPauseButtonClicked()
    {
        UIManager.Instance.OpenPanel("PausePanel");  // 打开暂停面板
    }
}