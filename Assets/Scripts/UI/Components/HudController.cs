using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class MovieShowInfo
{
    public string filmTitle;
    public string showTime;
    public string posterPath; // 海报路径
    public bool isActive; // 是否正在放映或即将放映
    public bool hasEnded; // 是否已结束
}

[System.Serializable]
public class ShowSlot
{
    public GameObject slotContainer; // 整个场次槽位的容器
    public Image posterImage;        // 海报图片
    public Text titleText;           // 电影标题
    public Text timeText;            // 放映时间
    public Text dateText;            // 日期文本（可选）
    public CanvasGroup canvasGroup;  // 用于淡入淡出效果
}

public class HudController : MonoBehaviour
{
    public Button pauseButton;          // 暂停按钮
    public Text moneyText;              // 显示金钱
    public Text timeText;               // 显示时间
    public Text audienceCountText;      // 显示观影人数
    
    [Header("电影场次显示")]
    public ShowSlot[] showSlots;        // 场次槽位数组
    public string posterResourcePath = "Posters/"; // 海报资源路径

    private EconomyManager economyManager;
    private ScheduleClock scheduleClock;
    private TicketGenerator ticketGenerator;
    private TicketQueueController ticketQueueController;
    
    private List<DaySchedule.Show> allShows = new List<DaySchedule.Show>();
    private List<DaySchedule.Show> activeShows = new List<DaySchedule.Show>();
    private List<DaySchedule.Show> endedShows = new List<DaySchedule.Show>();
    private Queue<DaySchedule.Show> pendingShows = new Queue<DaySchedule.Show>();

    private void Awake()
    {
        // 绑定按钮点击事件
        pauseButton.onClick.AddListener(OnPauseButtonClicked);

        // 获取相关管理器
        economyManager = FindObjectOfType<EconomyManager>();
        scheduleClock = FindObjectOfType<ScheduleClock>();
        ticketGenerator = FindObjectOfType<TicketGenerator>();
        ticketQueueController = FindObjectOfType<TicketQueueController>();
        
        // 初始化场次显示
        InitializeShowSlots();
    }

    private void Start()
    {
        // 获取所有场次信息
        InitializeShows();
        
        // 开始监听场次事件
        StartCoroutine(MonitorShows());
    }

    private void Update()
    {
        // 更新 HUD 上的金钱信息
        UpdateMoneyDisplay();
        
        // 更新 HUD 上的时间信息
        UpdateTimeDisplay();
        
        // 更新观影人数显示（新增）
        UpdateAudienceCountDisplay();
    }

    private void InitializeShowSlots()
    {
        // 隐藏所有槽位
        foreach (var slot in showSlots)
        {
            if (slot.slotContainer != null)
                slot.slotContainer.SetActive(false);
        }
    }

    private void InitializeShows()
    {
        DaySchedule currentDay = ticketGenerator.GetCurrentDay();
        if (currentDay != null)
        {
            allShows.Clear();
            allShows.AddRange(currentDay.shows);
            
            // 按时间排序
            allShows.Sort((a, b) => string.Compare(a.startTime, b.startTime, StringComparison.Ordinal));
            
            // 初始化待处理场次队列
            pendingShows.Clear();
            foreach (var show in allShows)
            {
                pendingShows.Enqueue(show);
            }
            
            activeShows.Clear();
            endedShows.Clear();
        }
    }

    private IEnumerator MonitorShows()
    {
        while (true)
        {
            UpdateActiveShows();
            UpdateShowDisplay();
            yield return new WaitForSeconds(1f); // 每秒检查一次
        }
    }

    private void UpdateActiveShows()
    {
        string currentTime = scheduleClock.GetCurrentGameTime();
        
        // 检查是否有新的场次可以激活
        while (pendingShows.Count > 0 && activeShows.Count < 3)
        {
            var nextShow = pendingShows.Peek();
            
            // 如果当前时间已经达到或超过场次开始时间，激活该场次
            if (string.Compare(currentTime, nextShow.startTime) >= 0)
            {
                activeShows.Add(pendingShows.Dequeue());
            }
            else
            {
                break; // 时间未到，等待
            }
        }
        
        // 检查已激活的场次是否结束
        for (int i = activeShows.Count - 1; i >= 0; i--)
        {
            var show = activeShows[i];
            
            DateTime showTime = DateTime.ParseExact(show.startTime, "HH:mm", null);
            DateTime current = DateTime.ParseExact(currentTime, "HH:mm", null);
            
            DateTime endTime = showTime.AddHours(2);
            
            if (current >= endTime && !endedShows.Contains(show))
            {
                endedShows.Add(show);
                activeShows.RemoveAt(i);
                
                // 如果有待处理的场次，立即激活一个
                if (pendingShows.Count > 0 && activeShows.Count < 3)
                {
                    activeShows.Add(pendingShows.Dequeue());
                }
            }
        }
    }

    private void UpdateShowDisplay()
    {
        // 隐藏所有槽位
        foreach (var slot in showSlots)
        {
            if (slot.slotContainer != null)
                slot.slotContainer.SetActive(false);
        }
        
        // 显示当前活跃的场次
        for (int i = 0; i < Mathf.Min(activeShows.Count, showSlots.Length); i++)
        {
            var show = activeShows[i];
            var slot = showSlots[i];
            
            if (slot.slotContainer != null)
            {
                slot.slotContainer.SetActive(true);
                UpdateShowSlot(slot, show);
            }
        }
    }

    private void UpdateShowSlot(ShowSlot slot, DaySchedule.Show show)
    {
        // 更新电影标题
        if (slot.titleText != null)
            slot.titleText.text = show.filmTitle;
        
        // 更新放映时间
        if (slot.timeText != null)
            slot.timeText.text = show.startTime;
        
        // 更新日期（可选，可以使用当前游戏日期）
        if (slot.dateText != null)
        {
            // 这里可以根据你的游戏时间系统显示具体日期
            slot.dateText.text = GetGameDate();
        }
        
        // 加载并显示海报
        if (slot.posterImage != null)
        {
            string posterName = show.filmTitle.Replace(" ", ""); // 移除空格作为资源名
            Sprite posterSprite = Resources.Load<Sprite>(posterResourcePath + posterName);
            
            if (posterSprite != null)
            {
                slot.posterImage.sprite = posterSprite;
            }
            else
            {
                Debug.LogWarning($"海报资源未找到: {posterResourcePath + posterName}");
                // 可以设置一个默认海报
            }
        }
        
        // 淡入效果
        if (slot.canvasGroup != null)
        {
            slot.canvasGroup.alpha = 1f;
        }
    }

    private string GetGameDate()
    {
        // 这里可以根据你的游戏时间系统返回当前日期
        // 例如：return System.DateTime.Now.ToString("MM/dd");
        return "Today";
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