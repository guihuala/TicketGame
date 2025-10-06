using System;

public enum SpecialEventType
{
    None,
    EarlyCheck,
    OldTicket,
    CopyTicket,
    DrawnTicket,
    DamagedTicket,
    WrongNameSpelling,
    MissingStub 
}

[Serializable]
public struct TicketData
{
    public string filmTitle;
    public string showTime;
    public string showDate;
    public SpecialEventType special;
    public bool hasStub;
    public bool isValid;
}