using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

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

    void Start()
    {
        currentDay = generator.GetCurrentDay();
        showIndex = 0;
        
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

        Debug.Log($"[TicketQueueController] 开始场次 {showIndex + 1}: {show.filmTitle} at {show.startTime}, 观众={show.audienceCount}");
        MsgCenter.SendMsg(MsgConst.MSG_SHOW_START, show.filmTitle, show.startTime);

        // 使用关卡配置的初始延迟
        Invoke(nameof(NextTicket), currentDay.initialTicketDelay);
    }
    
    private void NextTicket()
    {
        if (!showActive || waitingForPlayerInput) return;

        if (currentQueue.Count == 0)
        {
            bool onTime = scheduleClock.AllProcessedBeforeShowtime();
            Debug.Log($"[TicketQueueController] 场次 {showIndex + 1} 结束. 按时完成: {onTime}");
            MsgCenter.SendMsg(MsgConst.MSG_SHOW_END, onTime);
            showIndex++;
            showActive = false;
            
            // 使用关卡配置的场次间隔时间
            Invoke(nameof(StartShow), currentDay.timeBetweenShows);
            return;
        }

        currentTicket = currentQueue.Dequeue();

        // 实例化票UI
        currentTicketUI = Instantiate(ticketUIPrefab, ticketSpawnPoint.position, Quaternion.identity, parentForTickets);
        currentTicketUI.BindTicket(currentTicket);
        currentTicketUI.queue = this;

        // 使用关卡配置的滑入动画持续时间
        currentTicketUI.transform.DOMove(ticketDisplayPoint.position, currentDay.ticketSlideInDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                waitingForPlayerInput = true;
            });

        MsgCenter.SendMsg(MsgConst.MSG_TICKET_SPAWNED, currentTicket);
    }

    private void ProcessTicketResult(CheckResult result)
    {
        // 应用经济结果
        economy.ApplyResult(result);
        MsgCenter.SendMsg(MsgConst.MSG_TICKET_CHECKED, currentTicket, result);

        // 根据结果移动票
        Vector3 targetPosition = result.outcome == TicketOutcome.CorrectAccept || result.outcome == TicketOutcome.WrongAccept 
            ? ticketAcceptPoint.position 
            : ticketRejectPoint.position;

        // 使用关卡配置的滑出动画持续时间
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

        ProcessTicketResult(result);
    }
    
    public bool IsWaitingForInput()
    {
        return waitingForPlayerInput;
    }
}