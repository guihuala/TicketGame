public class MsgConst
{
    public const int MSG_GAME_START = 1;
    public const int MSG_GAME_PAUSED = 2;
    public const int MSG_GAME_OVER = 3;
    
    public const int MSG_TICKET_CHECKED = 4;
    public const int MSG_SHOW_START = 10;        // (string film, string time)
    public const int MSG_SHOW_END = 11;          // (bool finishedBeforeShowtime)
    public const int MSG_SCHEDULE_SET = 12;      // (film, time)
    public const int MSG_TICKET_SPAWNED = 13;    // (TicketData)
    public const int MSG_INCOME_CHANGED = 14;    // (int income, CheckResult result)
    
    public const int MSG_BROADCAST_DELAY = 21; 
    
    public const int MSG_SHOW_HINT = 31; // 显示提示信息
    public const int MSG_CAMERA_SHAKE = 32;
    public const int MSG_USE_UV_LIGHT = 33;
    public const int MSG_USE_VIP_PASS = 34;
}