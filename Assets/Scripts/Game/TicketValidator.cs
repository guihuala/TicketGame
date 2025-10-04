using UnityEngine;

public class TicketValidator : MonoBehaviour
{
    [Header("Penalties")] public int penaltyEarly = 5;
    public int penaltyInvalid = 10;

    public CheckResult ValidateAccept(TicketData t, ScheduleClock clock)
    {
        bool nameMatch = clock.IsFilmToday(t.filmTitle);
        bool timeMatch = clock.IsCorrectShowtime(t.showTime);
        bool tooEarly = clock.IsEarlierThanMinutes(t.showTime, 20);

        bool invalidSpecial = t.special == SpecialEventType.OldTicket
                              || t.special == SpecialEventType.CopyTicket
                              || t.special == SpecialEventType.DrawnTicket
                              || t.special == SpecialEventType.ElectronicAbuse
                              || t.special == SpecialEventType.DamagedTicket
                              || !t.hasStub;

        if (!nameMatch || !timeMatch || invalidSpecial)
            return new CheckResult
            {
                outcome = TicketOutcome.WrongAccept, incomeDelta = -penaltyInvalid, reason = "Invalid ticket admitted"
            };

        if (tooEarly || t.special == SpecialEventType.EarlyCheck)
            return new CheckResult
                { outcome = TicketOutcome.WrongAccept, incomeDelta = -penaltyEarly, reason = "Admitted too early" };

        return new CheckResult { outcome = TicketOutcome.CorrectAccept, incomeDelta = +1, reason = "Admit OK" };
    }

    public CheckResult ValidateReject(TicketData t, ScheduleClock clock)
    {
        bool nameMatch = clock.IsFilmToday(t.filmTitle);
        bool timeMatch = clock.IsCorrectShowtime(t.showTime);
        bool tooEarly = clock.IsEarlierThanMinutes(t.showTime, 20);

        bool invalidSpecial = t.special == SpecialEventType.OldTicket
                              || t.special == SpecialEventType.CopyTicket
                              || t.special == SpecialEventType.DrawnTicket
                              || t.special == SpecialEventType.ElectronicAbuse
                              || t.special == SpecialEventType.DamagedTicket
                              || !t.hasStub;

        if (!nameMatch || !timeMatch || invalidSpecial || tooEarly || t.special == SpecialEventType.EarlyCheck)
            return new CheckResult { outcome = TicketOutcome.CorrectReject, incomeDelta = 0, reason = "Reject OK" };

        return new CheckResult
            { outcome = TicketOutcome.WrongReject, incomeDelta = 0, reason = "Should have admitted" };
    }
}