using UnityEngine;

public class TicketValidator : MonoBehaviour
{
    [Header("Penalties")] 
    public int penaltyEarly = 5;      // 提前检票惩罚
    public int penaltyInvalid = 10;   // 其他无效票惩罚
    
    private int GetCurrentTicketPrice(TicketData ticket, ScheduleClock clock, DaySchedule currentDay)
    {
        if (currentDay == null) return 1;
        
        // 查找当前场次
        foreach (var show in currentDay.shows)
        {
            if (show.filmTitle == clock.CurrentFilm && show.startTime == clock.CurrentShowTime)
            {
                return show.ticketPrice;
            }
        }
        
        return 1; // 默认票价
    }

    public CheckResult ValidateAccept(TicketData t, ScheduleClock clock, DaySchedule currentDay = null)
    {
        bool nameMatch = clock.IsFilmToday(t.filmTitle);
        bool timeMatch = clock.IsCorrectShowtime(t.showTime);
        bool tooEarly = clock.IsEarlierThanMinutes(t.showTime, 20);
        
        // 1. 检查片名错误（电影没有在本影院放映）
        if (!nameMatch)
        {
            Debug.Log($"[ValidateAccept] 拒绝原因: 片名错误");
            return new CheckResult
            {
                outcome = TicketOutcome.WrongAccept, 
                incomeDelta = -penaltyInvalid, 
                reason = "Film not showing today"
            };
        }

        // 2. 检查时间不匹配
        if (!timeMatch)
        {
            Debug.Log($"[ValidateAccept] 拒绝原因: 时间不匹配");
            return new CheckResult
            {
                outcome = TicketOutcome.WrongAccept, 
                incomeDelta = -penaltyInvalid, 
                reason = "Wrong showtime"
            };
        }

        // 3. 检查提前检票（距离电影开场还有20分钟及以上）
        if (tooEarly)
        {
            Debug.Log($"[ValidateAccept] 拒绝原因: 提前检票");
            return new CheckResult
            { 
                outcome = TicketOutcome.WrongAccept, 
                incomeDelta = -penaltyEarly, 
                reason = "Admitted too early" 
            };
        }

        // 4. 检查票本身是否有效（根据特殊事件类型）
        bool isTicketValid = IsTicketValid(t);
        if (!isTicketValid)
        {
            Debug.Log($"[ValidateAccept] 拒绝原因: 无效票类型");
            return new CheckResult
            {
                outcome = TicketOutcome.WrongAccept, 
                incomeDelta = -penaltyInvalid, 
                reason = GetInvalidTicketReason(t)
            };
        }

        // 所有检查通过，正确接受 - 获得票价金额
        int income = GetCurrentTicketPrice(t, clock, currentDay);
        Debug.Log($"[ValidateAccept] 正确接受: 获得收入 +{income}");
        
        return new CheckResult 
        { 
            outcome = TicketOutcome.CorrectAccept, 
            incomeDelta = +income, 
            reason = $"Admit OK (+{income})" 
        };
    }

    public CheckResult ValidateReject(TicketData t, ScheduleClock clock, DaySchedule currentDay = null)
    {
        bool nameMatch = clock.IsFilmToday(t.filmTitle);
        bool timeMatch = clock.IsCorrectShowtime(t.showTime);
        bool tooEarly = clock.IsEarlierThanMinutes(t.showTime, 20);

        // 调试信息
        Debug.Log($"[ValidateReject] 票: {t.filmTitle} {t.showTime}, 特殊={t.special}, 有票根={t.hasStub}");

        // 1. 检查片名错误（电影没有在本影院放映）
        if (!nameMatch)
        {
            Debug.Log($"[ValidateReject] 正确拒绝: 片名错误");
            return new CheckResult 
            { 
                outcome = TicketOutcome.CorrectReject, 
                incomeDelta = 0, 
                reason = "Film not showing today" 
            };
        }

        // 2. 检查时间不匹配
        if (!timeMatch)
        {
            Debug.Log($"[ValidateReject] 正确拒绝: 时间不匹配");
            return new CheckResult 
            { 
                outcome = TicketOutcome.CorrectReject, 
                incomeDelta = 0, 
                reason = "Wrong showtime" 
            };
        }

        // 3. 检查提前检票
        if (tooEarly)
        {
            Debug.Log($"[ValidateReject] 正确拒绝: 提前检票");
            return new CheckResult 
            { 
                outcome = TicketOutcome.CorrectReject, 
                incomeDelta = 0, 
                reason = "Too early" 
            };
        }

        // 4. 检查票本身是否有效
        bool isTicketValid = IsTicketValid(t);
        if (!isTicketValid)
        {
            Debug.Log($"[ValidateReject] 正确拒绝: 无效票类型");
            return new CheckResult 
            { 
                outcome = TicketOutcome.CorrectReject, 
                incomeDelta = 0, 
                reason = GetInvalidTicketReason(t)
            };
        }

        // 如果票是有效的且匹配，但被拒绝了，这是错误的拒绝
        Debug.Log($"[ValidateReject] 错误拒绝: 应该接受的票");
        return new CheckResult
        { 
            outcome = TicketOutcome.WrongReject, 
            incomeDelta = 0, 
            reason = "Should have admitted" 
        };
    }

    /// <summary>
    /// 根据票的特殊事件类型判断票是否有效
    /// </summary>
    private bool IsTicketValid(TicketData ticket)
    {
        switch (ticket.special)
        {
            case SpecialEventType.None:
                // 正常票：有票根且信息正确就有效
                return ticket.hasStub;
                
            case SpecialEventType.EarlyCheck:
                // 提前检票：无效（根据规则1）
                return false;
                
            case SpecialEventType.OldTicket:
                // 旧影票：无效（根据规则2）
                return false;
                
            case SpecialEventType.WrongNameSpelling:
                // 错误命名：无效（根据规则7）
                return false;
                
            case SpecialEventType.DrawnTicket:
                // 画的影票：无效（根据规则4）
                return false;
                
            case SpecialEventType.CopyTicket:
            case SpecialEventType.DamagedTicket:
            case SpecialEventType.MissingStub:
                // 受损票/缺失票根：无效（根据规则6）
                return false;
                
            default:
                // 默认情况：有票根就有效
                return ticket.hasStub;
        }
    }

    /// <summary>
    /// 获取无效票的具体原因
    /// </summary>
    private string GetInvalidTicketReason(TicketData ticket)
    {
        switch (ticket.special)
        {
            case SpecialEventType.EarlyCheck:
                return "Early check-in not allowed";
            case SpecialEventType.OldTicket:
                return "Old ticket from previous show";
            case SpecialEventType.WrongNameSpelling:
                return "Wrong film name spelling";
            case SpecialEventType.DrawnTicket:
                return "Hand-drawn ticket";
            case SpecialEventType.DamagedTicket:
                return "Damaged ticket";
            case SpecialEventType.MissingStub:
                return "Missing ticket stub";
            case SpecialEventType.CopyTicket:
                return "Copied ticket";
            default:
                return "Invalid ticket";
        }
    }
}