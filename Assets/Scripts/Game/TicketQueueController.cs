using System.Collections.Generic;
using UnityEngine;

public class TicketQueueController : MonoBehaviour
{
    [SerializeField] private TicketGenerator generator;
    [SerializeField] private TicketValidator validator;
    [SerializeField] private TicketUI ticketUI;
    [SerializeField] private EconomyManager economy;
    [SerializeField] private ScheduleClock scheduleClock;

    private DaySchedule currentDay;
    private int showIndex;
    private Queue<TicketData> currentQueue;
    private TicketData currentTicket;
    private bool showActive;

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
            Debug.LogWarning("[TicketQueueController] No currentDay found, ending game");
            MsgCenter.SendMsgAct(MsgConst.MSG_GAME_OVER);
            return;
        }

        if (showIndex >= currentDay.shows.Count)
        {
            Debug.Log("[TicketQueueController] All shows finished for today");
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
        NextTicket();
    }

    private void NextTicket()
    {
        if (!showActive) return;

        if (currentQueue.Count == 0)
        {
            bool onTime = scheduleClock.AllProcessedBeforeShowtime();
            Debug.Log($"[TicketQueueController] Show {showIndex} finished. On time: {onTime}");
            MsgCenter.SendMsg(MsgConst.MSG_SHOW_END, onTime);
            showIndex++;
            showActive = false;
            StartShow();
            return;
        }

        currentTicket = currentQueue.Dequeue();
        Debug.Log($"[TicketQueueController] Spawned ticket: {currentTicket.filmTitle} {currentTicket.showTime} | Special={currentTicket.special}");
        ticketUI.BindTicket(currentTicket);
        MsgCenter.SendMsg(MsgConst.MSG_TICKET_SPAWNED, currentTicket);
    }

    public void AcceptCurrentTicket()
    {
        Debug.Log($"[TicketQueueController] Accepting ticket: {currentTicket.filmTitle} {currentTicket.showTime} | Special={currentTicket.special}");
        var result = validator.ValidateAccept(currentTicket, scheduleClock);
        Debug.Log($"[TicketQueueController] Result: {result.outcome}, Delta={result.incomeDelta}, Reason={result.reason}");
        economy.ApplyResult(result);
        MsgCenter.SendMsg(MsgConst.MSG_TICKET_CHECKED, currentTicket, result);
        NextTicket();
    }

    public void RejectCurrentTicket()
    {
        Debug.Log($"[TicketQueueController] Rejecting ticket: {currentTicket.filmTitle} {currentTicket.showTime} | Special={currentTicket.special}");
        var result = validator.ValidateReject(currentTicket, scheduleClock);
        Debug.Log($"[TicketQueueController] Result: {result.outcome}, Delta={result.incomeDelta}, Reason={result.reason}");
        economy.ApplyResult(result);
        MsgCenter.SendMsg(MsgConst.MSG_TICKET_CHECKED, currentTicket, result);
        NextTicket();
    }
}
