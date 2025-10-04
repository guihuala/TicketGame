using System;
using UnityEngine;
using UnityEngine.UI;

public class HudController : MonoBehaviour
{
    public Button pauseButton;  // 暂停按钮
    public Text moneyText;      // 显示金钱的 Text
    public Text timeText;       // 显示时间的 Text
    public Text scheduleText;   // 显示电影排期的 Text（新增）

    private EconomyManager economyManager;
    private ScheduleClock scheduleClock;
    private TicketGenerator ticketGenerator; // 引用 TicketGenerator

    private void Awake()
    {
        // 绑定按钮点击事件
        pauseButton.onClick.AddListener(OnPauseButtonClicked);

        // 获取相关管理器
        economyManager = FindObjectOfType<EconomyManager>();
        scheduleClock = FindObjectOfType<ScheduleClock>();
        ticketGenerator = FindObjectOfType<TicketGenerator>(); // 获取 TicketGenerator
    }

    private void Update()
    {
        // 更新 HUD 上的金钱信息
        UpdateMoneyDisplay();
        
        // 更新 HUD 上的时间信息
        UpdateTimeDisplay();

        // 更新当前电影排期
        UpdateScheduleDisplay();
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
    
    private void UpdateScheduleDisplay()
    {
        if (ticketGenerator != null && scheduleText != null)
        {
            // 获取当前关卡的排期信息
            DaySchedule currentDay = ticketGenerator.GetCurrentDay();  // 获取当前关卡
            if (currentDay != null)
            {
                string scheduleInfo = "Current Movie Schedule:\n";
                
                foreach (var show in currentDay.shows)
                {
                    scheduleInfo += $"{show.filmTitle} - {show.startTime}\n";  // 添加每个场次的电影名和时间
                }
                
                scheduleText.text = scheduleInfo;  // 更新电影排期文本
            }
        }
    }

    private void OnPauseButtonClicked()
    {
        UIManager.Instance.OpenPanel("PausePanel");  // 打开暂停面板
    }
}
