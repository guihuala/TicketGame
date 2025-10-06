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
    private bool countdownSoundPlayed = false;

    private bool isProcessingTicket = false;

    /// <summary>
    /// 由 GameManager 调用的初始化方法
    /// </summary>
    public void Initialize()
    {
        currentDay = generator.GetCurrentLevel();
        showIndex = 0;

        // 初始化统计
        totalAudienceCount = CalculateTotalAudienceCount();
        processedAudienceCount = 0;

        ApplyLevelTimeSettings();
        StartShow();

        Debug.Log("[TicketQueueController] 初始化完成");
    }

    private void ApplyLevelTimeSettings()
    {
        if (currentDay != null)
        {
            // 应用时间比例
            TimeManager.Instance.SetTimeFactor(currentDay.timeScale);
        }
    }

    /// <summary>
    /// 重置时间系统到当前关卡的开始时间
    /// </summary>
    private void ResetScheduleClock()
    {
        if (currentDay != null && scheduleClock != null)
        {
            scheduleClock.SetLevelStartTime(currentDay.levelStartTime);
            Debug.Log($"[TicketQueueController] 重置时间系统: {currentDay.levelStartTime}");
        }
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

        // 重置处理标志
        isProcessingTicket = false;

        // 使用关卡配置的滑入动画持续时间
        currentTicketUI.transform.DOMove(ticketDisplayPoint.position, currentDay.ticketSlideInDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() => { waitingForPlayerInput = true; });

        MsgCenter.SendMsg(MsgConst.MSG_TICKET_SPAWNED, currentTicket);
    }

    /// <summary>
    /// 跳到下一场电影开场前20分钟（仅在距离大于20分钟时跳转）
    /// </summary>
    private void JumpToNextShowTime()
    {
        if (showIndex >= currentDay.shows.Count) return;

        var nextShow = currentDay.shows[showIndex];

        try
        {
            // 解析下一场电影的开场时间
            var nextShowTime = DateTime.ParseExact(nextShow.startTime, "HH:mm", CultureInfo.InvariantCulture);

            // 获取当前游戏内时间
            var currentTime =
                DateTime.ParseExact(scheduleClock.GetCurrentGameTime(), "HH:mm", CultureInfo.InvariantCulture);

            // 计算时间差（分钟）
            TimeSpan timeDifference = nextShowTime - currentTime;
            double minutesUntilShow = timeDifference.TotalMinutes;

            // 只有当距离下一场开场大于20分钟时才进行时间跳转
            if (minutesUntilShow > 20)
            {
                // 计算跳到开场前20分钟的时间
                var targetTime = nextShowTime.AddMinutes(-20);

                // 将目标时间转换为秒数
                float targetSeconds = (float)(targetTime.TimeOfDay.TotalSeconds);

                // 设置时钟时间
                scheduleClock.simSeconds = targetSeconds;

                Debug.Log(
                    $"[TicketQueueController] 时间跳转: 跳到下一场 '{nextShow.filmTitle}' 开场前20分钟 ({targetTime:HH:mm})，时间跳转 {minutesUntilShow - 20:F1} 分钟");

                // 使用常规队列
                StartShow();
            }
            else
            {
                Debug.Log(
                    $"[TicketQueueController] 时间跳转跳过: 距离下一场 '{nextShow.filmTitle}' 只有 {minutesUntilShow:F1} 分钟，小于等于20分钟，使用混合票队列");

                // 使用混合票队列开始下一场
                StartShowWithMixedQueue();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[TicketQueueController] 时间跳转错误: {e.Message}");
            // 出错时使用默认延迟
            Invoke(nameof(StartShow), currentDay.timeBetweenShows);
        }
    }

    /// <summary>
    /// 使用混合票队列开始场次
    /// </summary>
    private void StartShowWithMixedQueue()
    {
        currentDay = generator.GetCurrentLevel();
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

        var show = currentDay.shows[showIndex];

        // 获取接下来20分钟内的所有场次
        var upcomingShows = generator.GetUpcomingShows(scheduleClock, 20);

        if (upcomingShows.Count > 0)
        {
            // 使用混合票队列
            currentQueue = generator.BuildMixedQueueForNext20Minutes(upcomingShows, scheduleClock);
            Debug.Log($"[TicketQueueController] 使用混合票队列，包含 {upcomingShows.Count} 个场次的票");
        }
        else
        {
            // 如果没有即将开始的场次，使用常规队列
            currentQueue = generator.BuildQueueForShow(show);
            Debug.Log($"[TicketQueueController] 使用常规票队列");
        }

        scheduleClock.SetTargetShow(show.filmTitle, show.startTime);
        showActive = true;

        MsgCenter.SendMsg(MsgConst.MSG_SHOW_START, show.filmTitle, show.startTime);
        
        // 显示场次开始提示
        string showHint = $"Scene {showIndex + 1}\n{show.filmTitle}\nStarts at {show.startTime}";
        MsgCenter.SendMsg(MsgConst.MSG_SHOW_HINT, showHint, 2.5f);
        
        AudioManager.Instance.PlaySfx("show");

        // 使用关卡配置的初始延迟
        Invoke(nameof(NextTicket), currentDay.initialTicketDelay);
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
        Vector3 targetPosition =
            result.outcome == TicketOutcome.CorrectAccept || result.outcome == TicketOutcome.WrongAccept
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
                isProcessingTicket = false; // 重置处理标志

                // 使用关卡配置的票间隔时间
                Invoke(nameof(NextTicket), currentDay.timeBetweenTickets);
            });
    }

    public void AcceptCurrentTicket()
    {
        // 防止重复处理：如果正在处理中或者票已经不在等待输入状态，直接返回
        if (isProcessingTicket || !waitingForPlayerInput || currentTicketUI == null)
            return;

        // 设置处理标志，防止重复点击
        isProcessingTicket = true;
        waitingForPlayerInput = false; // 立即禁用输入，防止再次点击

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
        // 防止重复处理：如果正在处理中或者票已经不在等待输入状态，直接返回
        if (isProcessingTicket || !waitingForPlayerInput || currentTicketUI == null)
            return;

        // 设置处理标志，防止重复点击
        isProcessingTicket = true;
        waitingForPlayerInput = false; // 立即禁用输入，防止再次点击

        Debug.Log(
            $"[TicketQueueController] 拒绝票: {currentTicket.filmTitle} {currentTicket.showTime} | 特殊={currentTicket.special}");

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
        return waitingForPlayerInput && !isProcessingTicket;
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

    /// <summary>
    /// 使用UV Light验证当前票的真伪
    /// </summary>
    public bool ValidateCurrentTicketWithUVLight()
    {
        if (currentTicketUI == null || !waitingForPlayerInput || isProcessingTicket)
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

    private void Update()
    {
        CheckShowTimeAndCountdown();
    }

    /// <summary>
    /// 检查电影开场时间和播放倒计时音效
    /// </summary>
    private void CheckShowTimeAndCountdown()
    {
        if (!showActive || currentDay == null || showIndex >= currentDay.shows.Count)
            return;

        var currentShow = currentDay.shows[showIndex];

        try
        {
            var showTime = DateTime.ParseExact(currentShow.startTime, "HH:mm", CultureInfo.InvariantCulture);
            var currentTime =
                DateTime.ParseExact(scheduleClock.GetCurrentGameTime(), "HH:mm", CultureInfo.InvariantCulture);

            double minutesUntilShow = (showTime - currentTime).TotalMinutes;

            // 检查是否错过开场时间（当前时间已经超过开场时间）
            if (minutesUntilShow < 0 && currentQueue != null && currentQueue.Count > 0)
            {
                // 错过开场时间，停止检票并清空剩余票
                StopCheckingForMissedShowtime();
                return;
            }

            // 检查是否应该播放倒计时音效（距离开场3分钟）
            if (minutesUntilShow <= 3 && minutesUntilShow > 0 && !countdownSoundPlayed)
            {
                AudioManager.Instance.PlaySfx("CountDown");
                countdownSoundPlayed = true;
            }

            // 重置倒计时音效标志（当时间超过3分钟时）
            if (minutesUntilShow > 3)
            {
                countdownSoundPlayed = false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[TicketQueueController] 检查开场时间错误: {e.Message}");
        }
    }

    /// <summary>
    /// 错过开场时间时的处理
    /// </summary>
    private void StopCheckingForMissedShowtime()
    {
        if (!showActive) return;

        // 清空当前队列
        currentQueue.Clear();

        // 如果当前有正在显示的票，移除它
        if (currentTicketUI != null)
        {
            Destroy(currentTicketUI.gameObject);
            currentTicketUI = null;
        }

        // 重置状态
        waitingForPlayerInput = false;
        isProcessingTicket = false;

        // 取消所有调用
        CancelInvoke(nameof(NextTicket));

        // 直接结束当前场次
        bool onTime = false; // 错过时间不算按时完成
        MsgCenter.SendMsg(MsgConst.MSG_SHOW_END, onTime);
        showIndex++;
        showActive = false;

        // 显示错过时间的提示
        string missedHint = "Showtime missed!\nRemaining tickets canceled.";
        MsgCenter.SendMsg(MsgConst.MSG_SHOW_HINT, missedHint, 3f);

        // 继续下一场或结束游戏
        if (showIndex < currentDay.shows.Count)
        {
            // 跳到下一场电影开场前20分钟
            Invoke(nameof(JumpToNextShowTime), 3f);
        }
        else
        {
            // 所有场次结束
            string endHint = "All sessions completed!\nCalculating results...";
            MsgCenter.SendMsg(MsgConst.MSG_SHOW_HINT, endHint, 2f);
            Invoke(nameof(EndAllShows), 3f);
        }
    }

    /// <summary>
    /// 结束所有场次
    /// </summary>
    private void EndAllShows()
    {
        MsgCenter.SendMsgAct(MsgConst.MSG_GAME_OVER);
        GameManager.Instance.EndGame();
    }
    
    private void StartShow()
    {
        currentDay = generator.GetCurrentLevel();
        if (currentDay == null)
        {
            MsgCenter.SendMsgAct(MsgConst.MSG_GAME_OVER);
            return;
        }

        if (showIndex == 0)
        {
            ResetScheduleClock();
        }

        if (showIndex >= currentDay.shows.Count)
        {
            MsgCenter.SendMsgAct(MsgConst.MSG_GAME_OVER);
            GameManager.Instance.EndGame();
            return;
        }

        // 重置倒计时音效标志
        countdownSoundPlayed = false;

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
}