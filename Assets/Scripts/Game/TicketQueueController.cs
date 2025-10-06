using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using System.Globalization;

public class TicketQueueController : MonoBehaviour
{
    [SerializeField] private TicketGenerator generator;
    [SerializeField] private TicketValidator validator;
    [SerializeField] private TicketUI ticketUIPrefab;
    [SerializeField] private EconomyManager economy;
    [SerializeField] private ScheduleClock scheduleClock;
    [SerializeField] private Transform ticketSpawnPoint;
    [SerializeField] private Transform ticketDisplayPoint;
    [SerializeField] private Transform ticketAcceptPoint;
    [SerializeField] private Transform ticketRejectPoint;
    [SerializeField] private Transform parentForTickets;

    private DaySchedule currentDay;
    private int showIndex;
    private Queue<TicketData> currentQueue;
    private TicketData currentTicket;
    private TicketUI currentTicketUI;
    private bool showActive;
    private bool waitingForPlayerInput = false;
    private int totalAudienceCount = 0;
    private int processedAudienceCount = 0;

    void Start()
    {
        currentDay = generator.GetCurrentDay();
        showIndex = 0;
        
        // 初始化统计
        totalAudienceCount = CalculateTotalAudienceCount();
        processedAudienceCount = 0;
        
        ApplyLevelTimeSettings();
        StartShow();
    }

    private void ApplyLevelTimeSettings()
    {
        if (currentDay != null)
        {
            // 应用时间比例
            TimeManager.Instance.SetTimeFactor(currentDay.timeScale);
        }
    }
    
    private void StartShow()
    {
        currentDay = generator.GetCurrentDay();
        if (currentDay == null)
        {
            MsgCenter.SendMsgAct(MsgConst.MSG_GAME_OVER);
            return;
        }

        if (showIndex >= currentDay.shows.Count)
        {
            MsgCenter.SendMsgAct(MsgConst.MSG_GAME_OVER);
            GameManager.Instance.EndGame();
            return;
        }

        // 设置关卡开始时间（只在第一场开始时设置）
        if (showIndex == 0)
        {
            scheduleClock.SetLevelStartTime(currentDay.levelStartTime);
        }

        var show = currentDay.shows[showIndex];
        currentQueue = generator.BuildQueueForShow(show);
        scheduleClock.SetTargetShow(show.filmTitle, show.startTime);
        showActive = true;
        
        MsgCenter.SendMsg(MsgConst.MSG_SHOW_START, show.filmTitle, show.startTime);

        // 显示场次开始提示
        string showHint = $"Scene {showIndex + 1}\n{show.filmTitle}\nStarts at {show.startTime}";
        MsgCenter.SendMsg(MsgConst.MSG_SHOW_HINT, showHint, 2.5f);

        // 使用关卡配置的初始延迟
        Invoke(nameof(NextTicket), currentDay.initialTicketDelay);
    }
    
    private void NextTicket()
    {
        if (!showActive || waitingForPlayerInput) return;

        if (currentQueue.Count == 0)
        {
            bool onTime = scheduleClock.AllProcessedBeforeShowtime();
            MsgCenter.SendMsg(MsgConst.MSG_SHOW_END, onTime);
            showIndex++;
            showActive = false;
        
            // 显示中场休息提示或游戏结束提示
            if (showIndex < currentDay.shows.Count)
            {
                string breakHint = "Intermission\nPrepare for the next movie...";
                MsgCenter.SendMsg(MsgConst.MSG_SHOW_HINT, breakHint, 2f);
                
                // 跳到下一场电影开场前20分钟
                JumpToNextShowTime();
            }
            else
            {
                string endHint = "All sessions completed! \nCalculating results...";
                MsgCenter.SendMsg(MsgConst.MSG_SHOW_HINT, endHint, 2f);
                
                // 所有场次结束，使用关卡配置的场次间隔时间
                Invoke(nameof(StartShow), currentDay.timeBetweenShows);
            }
            return;
        }

        currentTicket = currentQueue.Dequeue();

        // 实例化票UI
        currentTicketUI = Instantiate(ticketUIPrefab, ticketSpawnPoint.position, Quaternion.identity, parentForTickets);
        currentTicketUI.BindTicket(currentTicket);
        currentTicketUI.queue = this;
        
        AudioManager.Instance.PlaySfx("Ticket_in");

        // 使用关卡配置的滑入动画持续时间
        currentTicketUI.transform.DOMove(ticketDisplayPoint.position, currentDay.ticketSlideInDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                waitingForPlayerInput = true;
            });

        MsgCenter.SendMsg(MsgConst.MSG_TICKET_SPAWNED, currentTicket);
    }

    /// <summary>
    /// 跳到下一场电影开场前20分钟
    /// </summary>
    private void JumpToNextShowTime()
    {
        if (showIndex >= currentDay.shows.Count) return;
        
        var nextShow = currentDay.shows[showIndex];
        
        try
        {
            // 解析下一场电影的开场时间
            var nextShowTime = DateTime.ParseExact(nextShow.startTime, "HH:mm", CultureInfo.InvariantCulture);
            
            // 计算跳到开场前20分钟的时间
            var targetTime = nextShowTime.AddMinutes(-20);
            
            // 将目标时间转换为秒数
            float targetSeconds = (float)(targetTime.TimeOfDay.TotalSeconds);
            
            // 设置时钟时间
            scheduleClock.simSeconds = targetSeconds;
            
            Debug.Log($"[TicketQueueController] 时间跳转: 跳到下一场 '{nextShow.filmTitle}' 开场前20分钟 ({targetTime:HH:mm})");
            
            // 立即开始下一场（不使用延迟）
            StartShow();
        }
        catch (Exception e)
        {
            Debug.LogError($"[TicketQueueController] 时间跳转错误: {e.Message}");
            // 出错时使用默认延迟
            Invoke(nameof(StartShow), currentDay.timeBetweenShows);
        }
    }

    private void ProcessTicketResult(CheckResult result)
    {
        processedAudienceCount++;
    
        // 应用经济结果
        economy.ApplyResult(result);
        MsgCenter.SendMsg(MsgConst.MSG_TICKET_CHECKED, currentTicket, result);

        // 显示浮动文本
        if (FloatingTextManager.Instance != null && currentTicketUI != null)
        {
            Vector3 worldPosition = currentTicketUI.transform.position;
            FloatingTextManager.Instance.ShowMoneyChange(result.incomeDelta, worldPosition);
        }

        // 根据结果移动票
        Vector3 targetPosition = result.outcome == TicketOutcome.CorrectAccept || result.outcome == TicketOutcome.WrongAccept 
            ? ticketAcceptPoint.position 
            : ticketRejectPoint.position;
        
        AudioManager.Instance.PlaySfx("Ticket_out");
        
        currentTicketUI.transform.DOMove(targetPosition, currentDay.ticketSlideOutDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                if (currentTicketUI != null)
                {
                    Destroy(currentTicketUI.gameObject);
                    currentTicketUI = null;
                }

                waitingForPlayerInput = false;
            
                // 使用关卡配置的票间隔时间
                Invoke(nameof(NextTicket), currentDay.timeBetweenTickets);
            });
    }
    
    public void AcceptCurrentTicket()
    {
        if (!waitingForPlayerInput || currentTicketUI == null) return;

        // 传递当前关卡数据给验证器
        var result = validator.ValidateAccept(currentTicket, scheduleClock, currentDay);
        
        // 根据验票结果播放对应的音效
        if (result.outcome == TicketOutcome.CorrectAccept)
        {
            AudioManager.Instance.PlaySfx("Success");
        }
        else if (result.outcome == TicketOutcome.WrongAccept)
        {
            AudioManager.Instance.PlaySfx("Wrong");
        }
        
        // 触发撕票动画
        if (currentTicketUI != null)
        {
            var visual = currentTicketUI.GetComponent<TicketVisual>();
            if (visual != null)
            {
                visual.OnTearSuccess();
            }
        }

        ProcessTicketResult(result);
    }

    public void RejectCurrentTicket()
    {
        if (!waitingForPlayerInput || currentTicketUI == null) return;

        Debug.Log($"[TicketQueueController] 拒绝票: {currentTicket.filmTitle} {currentTicket.showTime} | 特殊={currentTicket.special}");
    
        // 传递当前关卡数据给验证器
        var result = validator.ValidateReject(currentTicket, scheduleClock, currentDay);
        Debug.Log($"[TicketQueueController] 结果: {result.outcome}, 收入变化={result.incomeDelta}, 原因={result.reason}");
        
        // 根据验票结果播放对应的音效
        if (result.outcome == TicketOutcome.CorrectReject)
        {
            AudioManager.Instance.PlaySfx("Success");
        }
        else if (result.outcome == TicketOutcome.WrongReject)
        {
            AudioManager.Instance.PlaySfx("Wrong");
        }
        
        ProcessTicketResult(result);
    }
    
    public bool IsWaitingForInput()
    {
        return waitingForPlayerInput;
    }
    
    private int CalculateTotalAudienceCount()
    {
        if (currentDay == null) return 0;
        
        int total = 0;
        foreach (var show in currentDay.shows)
        {
            total += show.audienceCount;
        }
        return total;
    }
    
    public int GetTotalAudienceCount()
    {
        return totalAudienceCount;
    }
    
    public int GetProcessedAudienceCount()
    {
        return processedAudienceCount;
    }
    
    // 在 TicketQueueController.cs 中添加以下方法：

    /// <summary>
    /// 使用UV Light验证当前票的真伪
    /// </summary>
    public bool ValidateCurrentTicketWithUVLight()
    {
        if (currentTicketUI == null || !waitingForPlayerInput) 
            return false;
    
        // 调用验证器检查票是否有效
        return IsCurrentTicketValid();
    }

    /// <summary>
    /// 检查当前票是否有效
    /// </summary>
    private bool IsCurrentTicketValid()
    {
        if (currentTicket.special == SpecialEventType.None)
        {
            // 正常票：有票根且信息正确就有效
            return currentTicket.hasStub && currentTicket.isValid;
        }
    
        // 根据特殊事件类型判断有效性
        switch (currentTicket.special)
        {
            case SpecialEventType.EarlyCheck:
            case SpecialEventType.OldTicket:
            case SpecialEventType.WrongNameSpelling:
            case SpecialEventType.DrawnTicket:
            case SpecialEventType.CopyTicket:
                // 这些类型的票都是无效的
                return false;
            
            case SpecialEventType.DamagedTicket:
            case SpecialEventType.MissingStub:
                // 受损票和缺失票根：信息正确但票本身有问题
                return currentTicket.isValid; // 使用配置的是否应该接受
            
            default:
                return currentTicket.hasStub && currentTicket.isValid;
        }
    }

    /// <summary>
    /// 显示UV Light验证结果
    /// </summary>
    public void ShowUVLightResult(bool isValid)
    {
        if (currentTicketUI != null)
        {
            currentTicketUI.ShowUVLightEffect(isValid);
        }
    }
}