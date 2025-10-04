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
    [SerializeField] private Transform ticketDisplayPoint; // 票显示位置（玩家面前）
    [SerializeField] private Transform ticketAcceptPoint; // 接受后的位置
    [SerializeField] private Transform ticketRejectPoint; // 拒绝后的位置
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
        StartShow();
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

        var show = currentDay.shows[showIndex];
        currentQueue = generator.BuildQueueForShow(show);
        scheduleClock.SetTargetShow(show.filmTitle, show.startTime);
        showActive = true;

        Debug.Log($"[TicketQueueController] Starting show {showIndex}: {show.filmTitle} at {show.startTime}, Audience={show.audienceCount}");
        MsgCenter.SendMsg(MsgConst.MSG_SHOW_START, show.filmTitle, show.startTime);
        
        // 延迟一会儿再生成第一张票
        Invoke(nameof(NextTicket), 1f);
    }

    private void NextTicket()
    {
        if (!showActive || waitingForPlayerInput) return;

        if (currentQueue.Count == 0)
        {
            bool onTime = scheduleClock.AllProcessedBeforeShowtime();
            Debug.Log($"[TicketQueueController] Show {showIndex} finished. On time: {onTime}");
            MsgCenter.SendMsg(MsgConst.MSG_SHOW_END, onTime);
            showIndex++;
            showActive = false;
            
            // 延迟一会儿再开始下一场
            Invoke(nameof(StartShow), 2f);
            return;
        }

        currentTicket = currentQueue.Dequeue();

        // 实例化票UI
        currentTicketUI = Instantiate(ticketUIPrefab, ticketSpawnPoint.position, Quaternion.identity, parentForTickets);
        currentTicketUI.BindTicket(currentTicket);
        currentTicketUI.queue = this;

        // 动画：从左侧滑入到显示位置
        currentTicketUI.transform.DOMove(ticketDisplayPoint.position, 0.5f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                // 到达显示位置后，等待玩家输入
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

        // 动画：移动到接受/拒绝位置并销毁
        currentTicketUI.transform.DOMove(targetPosition, 0.5f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                // 销毁当前票
                if (currentTicketUI != null)
                {
                    Destroy(currentTicketUI.gameObject);
                    currentTicketUI = null;
                }

                // 重置状态，准备下一张票
                waitingForPlayerInput = false;
                
                // 延迟一会儿再生成下一张票
                Invoke(nameof(NextTicket), 0.5f);
            });
    }

    public void AcceptCurrentTicket()
    {
        if (!waitingForPlayerInput || currentTicketUI == null) return;

        Debug.Log($"[TicketQueueController] Accepting ticket: {currentTicket.filmTitle} {currentTicket.showTime} | Special={currentTicket.special}");
        var result = validator.ValidateAccept(currentTicket, scheduleClock);
        Debug.Log($"[TicketQueueController] Result: {result.outcome}, Delta={result.incomeDelta}, Reason={result.reason}");
        
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

        var result = validator.ValidateReject(currentTicket, scheduleClock);

        ProcessTicketResult(result);
    }

    // 获取当前是否在等待玩家输入
    public bool IsWaitingForInput()
    {
        return waitingForPlayerInput;
    }
}