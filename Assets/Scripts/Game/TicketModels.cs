using System;

public enum SpecialEventType
{
    None,
    EarlyCheck,
    OldTicket,
    CopyTicket,
    DrawnTicket,
    ElectronicAbuse,
    DamagedTicket
}

[Serializable]
public struct TicketData
{
    public string filmTitle;
    public string showTime;
    public SpecialEventType special;
    public bool hasStub;
    public bool isValid;
}
