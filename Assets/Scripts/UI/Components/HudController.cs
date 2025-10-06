using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    
    private string currentLevelDate; // 存储当前关卡日期

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
        // 获取当前关卡日期
        UpdateCurrentLevelDate();
        
        // 获取所有场次信息
        InitializeShows();
        
        // 注册消息监听
        MsgCenter.RegisterMsg(MsgConst.MSG_SHOW_END, OnShowEnded);
        MsgCenter.RegisterMsg(MsgConst.MSG_SHOW_START, OnShowStarted);
        
        // 开始监听场次事件
        StartCoroutine(MonitorShows());
    }

    private void Update()
    {
        UpdateMoneyDisplay();
        UpdateAudienceCountDisplay();
        UpdateTimeDisplay();
    }

    private void OnDestroy()
    {
        // 注销消息监听
        MsgCenter.UnregisterMsg(MsgConst.MSG_SHOW_END, OnShowEnded);
        MsgCenter.UnregisterMsg(MsgConst.MSG_SHOW_START, OnShowStarted);
    }

    /// <summary>
    /// 更新当前关卡日期
    /// </summary>
    private void UpdateCurrentLevelDate()
    {
        DaySchedule currentDay = ticketGenerator.GetCurrentLevel();
        if (currentDay != null)
        {
            currentLevelDate = currentDay.levelDate;
            Debug.Log($"[HudController] 当前关卡日期: {currentLevelDate}");
        }
        else
        {
            currentLevelDate = "04/10/25"; // 默认日期
            Debug.LogWarning($"[HudController] 无法获取关卡日期，使用默认值: {currentLevelDate}");
        }
    }

    /// <summary>
    /// 处理场次结束事件
    /// </summary>
    private void OnShowEnded(params object[] parameters)
    {
        if (parameters.Length > 0 && parameters[0] is bool onTime)
        {
            Debug.Log($"[HudController] 场次结束，是否按时完成: {onTime}");
            
            // 立即更新场次显示
            UpdateActiveShows();
            UpdateShowDisplay();
        }
    }

    /// <summary>
    /// 处理场次开始事件
    /// </summary>
    private void OnShowStarted(params object[] parameters)
    {
        if (parameters.Length >= 2)
        {
            string filmTitle = parameters[0] as string;
            string startTime = parameters[1] as string;
            Debug.Log($"[HudController] 场次开始: {filmTitle} {startTime}");
            
            // 立即更新场次显示
            UpdateActiveShows();
            UpdateShowDisplay();
        }
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
        DaySchedule currentDay = ticketGenerator.GetCurrentLevel();
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
        else
        {
            Debug.LogError("[HudController] 无法获取当前关卡数据");
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

        // 清空当前活跃场次，重新计算接下来60分钟内的场次
        activeShows.Clear();

        // 检查所有待处理场次，找出接下来60分钟内的场次
        var tempPendingShows = new Queue<DaySchedule.Show>(pendingShows);
        var showsToActivate = new List<DaySchedule.Show>();

        while (tempPendingShows.Count > 0 && activeShows.Count + showsToActivate.Count < 3)
        {
            var nextShow = tempPendingShows.Dequeue();

            // 计算当前时间与场次开始时间的时间差（分钟）
            DateTime current = DateTime.ParseExact(currentTime, "HH:mm", null);
            DateTime showTime = DateTime.ParseExact(nextShow.startTime, "HH:mm", null);

            // 如果场次在当前时间之后60分钟内，则激活显示
            if (showTime >= current && (showTime - current).TotalMinutes <= 60)
            {
                showsToActivate.Add(nextShow);
            }
            else if ((showTime - current).TotalMinutes > 60)
            {
                // 超过60分钟的场次，停止检查
                break;
            }
        }

        // 将符合条件的场次添加到活跃列表
        foreach (var show in showsToActivate)
        {
            activeShows.Add(show);
        }

        // 检查已激活的场次是否已经结束（开始后2小时）
        for (int i = activeShows.Count - 1; i >= 0; i--)
        {
            var show = activeShows[i];

            DateTime showTime = DateTime.ParseExact(show.startTime, "HH:mm", null);
            DateTime current = DateTime.ParseExact(currentTime, "HH:mm", null);

            DateTime endTime = showTime.AddHours(2);

            if (current >= endTime)
            {
                // 从活跃列表中移除已结束的场次
                if (!endedShows.Contains(show))
                {
                    endedShows.Add(show);
                }

                activeShows.RemoveAt(i);
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
        
        // 如果没有活跃场次，显示提示或隐藏所有
        if (activeShows.Count == 0 && pendingShows.Count == 0)
        {
            Debug.Log("[HudController] 所有场次已结束");
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
        
        // 更新日期 - 使用关卡配置的日期
        if (slot.dateText != null)
        {
            slot.dateText.text = currentLevelDate;
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
        
            // 获取当前关卡日期
            DaySchedule currentDay = ticketGenerator.GetCurrentLevel();
            if (currentDay != null && !string.IsNullOrEmpty(currentDay.levelDate))
            {
                try
                {
                    // 解析关卡日期
                    string[] dateParts = currentDay.levelDate.Split('/');
                    if (dateParts.Length == 3)
                    {
                        string month = dateParts[0];
                        string day = dateParts[1];
                        string year = dateParts[2];
                    
                        // 格式：月份/日/年 时:分
                        timeText.text = $"{month}/{day}/{year} {time.Hours:D2}:{time.Minutes:D2}";
                    }
                    else
                    {
                        // 如果日期格式不正确，使用默认格式
                        timeText.text = $"{time.Hours:D2}:{time.Minutes:D2}";
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"时间显示格式化错误: {e.Message}");
                    timeText.text = $"{time.Hours:D2}:{time.Minutes:D2}";
                }
            }
            else
            {
                // 如果没有关卡日期，只显示时间
                timeText.text = $"{time.Hours:D2}:{time.Minutes:D2}";
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