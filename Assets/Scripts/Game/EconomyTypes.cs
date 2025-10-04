public enum TicketOutcome
{
    CorrectAccept,
    CorrectReject,
    WrongAccept,
    WrongReject
}

public struct CheckResult
{
    public TicketOutcome outcome;
    public int incomeDelta;
    public string reason;
}
