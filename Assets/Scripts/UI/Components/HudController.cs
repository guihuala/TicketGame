using System;
using UnityEngine;
using UnityEngine.UI;

public class HudController : MonoBehaviour
{
    public Button pauseButton;          // 暂停按钮
    public Text moneyText;              // 显示金钱
    public Text timeText;               // 显示时间
    public Text scheduleText;           // 显示电影排期
    public Text audienceCountText;      // 显示观影人数

    private EconomyManager economyManager;
    private ScheduleClock scheduleClock;
    private TicketGenerator ticketGenerator;
    private TicketQueueController ticketQueueController;

    private void Awake()
    {
        // 绑定按钮点击事件
        pauseButton.onClick.AddListener(OnPauseButtonClicked);

        // 获取相关管理器
        economyManager = FindObjectOfType<EconomyManager>();
        scheduleClock = FindObjectOfType<ScheduleClock>();
        ticketGenerator = FindObjectOfType<TicketGenerator>();
        ticketQueueController = FindObjectOfType<TicketQueueController>();
    }

    private void Update()
    {
        // 更新 HUD 上的金钱信息
        UpdateMoneyDisplay();
        
        // 更新 HUD 上的时间信息
        UpdateTimeDisplay();

        // 更新当前电影排期
        UpdateScheduleDisplay();
        
        // 更新观影人数显示（新增）
        UpdateAudienceCountDisplay();
    }

    private void UpdateMoneyDisplay()
    {
        if (economyManager != null && moneyText != null)
        {
            moneyText.text = "Money: $" + economyManager.currentIncome.ToString();
        }
    }

    private void UpdateTimeDisplay()
    {
        if (scheduleClock != null && timeText != null)
        {
            TimeSpan time = TimeSpan.FromSeconds(scheduleClock.simSeconds);
            timeText.text = time.ToString(@"hh\:mm");
        }
    }
    
    private void UpdateScheduleDisplay()
    {
        if (ticketGenerator != null && scheduleText != null)
        {
            DaySchedule currentDay = ticketGenerator.GetCurrentDay();
            if (currentDay != null)
            {
                string scheduleInfo = "Current Movie Schedule:\n";
                
                foreach (var show in currentDay.shows)
                {
                    scheduleInfo += $"{show.filmTitle} - {show.startTime}\n";
                }
                
                scheduleText.text = scheduleInfo;
            }
        }
    }
    
    private void UpdateAudienceCountDisplay()
    {
        if (ticketQueueController != null && audienceCountText != null)
        {
            int totalAudience = ticketQueueController.GetTotalAudienceCount();
            int processedAudience = ticketQueueController.GetProcessedAudienceCount();
            audienceCountText.text = $"{totalAudience - processedAudience}";
        }
    }

    private void OnPauseButtonClicked()
    {
        UIManager.Instance.OpenPanel("PausePanel");
    }
}