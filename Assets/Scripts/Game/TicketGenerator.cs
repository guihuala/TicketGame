using System.Collections.Generic;
using UnityEngine;

public class TicketGenerator : MonoBehaviour
{
    [SerializeField] private LevelDatabase database;
    [SerializeField] private int currentLevelIndex = 0;
    
    // 月份映射表
    private Dictionary<string, string> monthMapping = new Dictionary<string, string>
    {
        { "01", "January" },
        { "02", "February" },
        { "03", "March" },
        { "04", "April" },
        { "05", "May" },
        { "06", "June" },
        { "07", "July" },
        { "08", "August" },
        { "09", "September" },
        { "10", "October" },
        { "11", "November" },
        { "12", "December" }
    };
    
    public Queue<TicketData> BuildQueueForShow(DaySchedule.Show show)
    {
        var q = new Queue<TicketData>();
        var specialTickets = new List<TicketData>();

        // 使用新的特殊事件配置系统
        foreach (var specialConfig in show.specialEvents)
        {
            specialTickets.AddRange(CreateSpecialTickets(specialConfig, show));
        }

        // 计算正常票数量
        int normalTicketCount = show.audienceCount - specialTickets.Count;
        if (normalTicketCount < 0)
        {
            Debug.LogWarning($"特殊票数量({specialTickets.Count})超过观众总数({show.audienceCount})，调整特殊票数量");
            while (specialTickets.Count > show.audienceCount)
            {
                specialTickets.RemoveAt(Random.Range(0, specialTickets.Count));
            }
            normalTicketCount = 0;
        }

        // 创建正常票
        for (int i = 0; i < normalTicketCount; i++)
        {
            var normalTicket = CreateNormalTicket(show);
            q.Enqueue(normalTicket);
        }

        // 混入特殊票
        foreach (var specialTicket in specialTickets)
        {
            q.Enqueue(specialTicket);
        }

        // 打乱队列顺序
        q = ShuffleQueue(q);

        Debug.Log($"[TicketGenerator] 场次 {show.startTime} {show.filmTitle}: 正常票={normalTicketCount}, 特殊票={specialTickets.Count}");
        return q;
    }
    
    /// <summary>
    /// 为接下来20分钟内的所有电影生成混合票队列
    /// </summary>
    public Queue<TicketData> BuildMixedQueueForNext20Minutes(List<DaySchedule.Show> upcomingShows, ScheduleClock clock)
    {
        var mixedQueue = new Queue<TicketData>();
        var allTickets = new List<TicketData>();
        
        // 为每个即将开始的场次生成票
        foreach (var show in upcomingShows)
        {
            // 计算这个场次应该生成的票数（基于时间比例）
            int ticketCountForShow = CalculateTicketCountForUpcomingShow(show, clock);
            if (ticketCountForShow <= 0) continue;
            
            // 生成这个场次的票（包括特殊票）
            var showTickets = GenerateTicketsForUpcomingShow(show, ticketCountForShow);
            allTickets.AddRange(showTickets);
            
            Debug.Log($"[TicketGenerator] 为即将开始的场次 '{show.filmTitle}' ({show.startTime}) 生成 {showTickets.Count} 张票");
        }
        
        // 打乱所有票的顺序
        allTickets = ShuffleList(allTickets);
        
        // 放入队列
        foreach (var ticket in allTickets)
        {
            mixedQueue.Enqueue(ticket);
        }
        
        Debug.Log($"[TicketGenerator] 为接下来20分钟内的 {upcomingShows.Count} 个场次生成混合票队列，总计 {mixedQueue.Count} 张票");
        return mixedQueue;
    }
    
    /// <summary>
    /// 计算为即将开始的场次生成多少张票
    /// </summary>
    private int CalculateTicketCountForUpcomingShow(DaySchedule.Show show, ScheduleClock clock)
    {
        try
        {
            var showTime = System.DateTime.ParseExact(show.startTime, "HH:mm", System.Globalization.CultureInfo.InvariantCulture);
            var currentTime = System.DateTime.ParseExact(clock.GetCurrentGameTime(), "HH:mm", System.Globalization.CultureInfo.InvariantCulture);
            
            double minutesUntilShow = (showTime - currentTime).TotalMinutes;
            
            // 基于距离开场时间计算票数比例
            // 距离越近，生成的票越多
            float ticketRatio = Mathf.Clamp01(1f - (float)(minutesUntilShow / 20f));
            
            int ticketCount = Mathf.RoundToInt(show.audienceCount * ticketRatio);
            
            // 确保至少生成1张票（如果有观众的话）
            ticketCount = Mathf.Clamp(ticketCount, 0, show.audienceCount);
            
            return ticketCount;
        }
        catch
        {
            // 如果解析时间出错，使用默认值
            return Mathf.RoundToInt(show.audienceCount * 0.5f);
        }
    }
    
    /// <summary>
    /// 为即将开始的场次生成票
    /// </summary>
    private List<TicketData> GenerateTicketsForUpcomingShow(DaySchedule.Show show, int ticketCount)
    {
        var tickets = new List<TicketData>();
        
        // 计算特殊票数量（按比例）
        int specialTicketCount = 0;
        foreach (var specialConfig in show.specialEvents)
        {
            // 按比例减少特殊票数量
            int adjustedSpecialCount = Mathf.RoundToInt(specialConfig.count * ((float)ticketCount / show.audienceCount));
            adjustedSpecialCount = Mathf.Clamp(adjustedSpecialCount, 0, specialConfig.count);
            
            for (int i = 0; i < adjustedSpecialCount; i++)
            {
                var specialTicket = CreateSpecialTicket(specialConfig, show);
                tickets.Add(specialTicket);
                specialTicketCount++;
            }
        }
        
        // 计算正常票数量
        int normalTicketCount = ticketCount - specialTicketCount;
        if (normalTicketCount < 0)
        {
            // 如果特殊票过多，调整数量
            while (tickets.Count > ticketCount)
            {
                tickets.RemoveAt(Random.Range(0, tickets.Count));
            }
            normalTicketCount = 0;
        }
        
        // 生成正常票
        for (int i = 0; i < normalTicketCount; i++)
        {
            var normalTicket = CreateNormalTicket(show);
            tickets.Add(normalTicket);
        }
        
        return tickets;
    }
    
    private List<TicketData> CreateSpecialTickets(DaySchedule.SpecialEventConfig config, DaySchedule.Show show)
    {
        var list = new List<TicketData>();
        for (int i = 0; i < config.count; i++)
        {
            var ticket = CreateSpecialTicket(config, show);
            list.Add(ticket);
        }
        return list;
    }

    private TicketData CreateNormalTicket(DaySchedule.Show show)
    {
        var currentDay = GetCurrentLevel();
        string date = currentDay != null ? currentDay.levelDate : "04/10/25";
        string formattedDate = FormatDateToEnglish(date);
        
        return new TicketData
        {
            filmTitle = show.filmTitle,
            showTime = show.startTime,
            showDate = formattedDate, // 使用格式化后的日期
            special = SpecialEventType.None,
            hasStub = true,
            isValid = true  // 正常票默认有效
        };
    }

    private TicketData CreateSpecialTicket(DaySchedule.SpecialEventConfig config, DaySchedule.Show show)
    {
        var currentDay = GetCurrentLevel();
        string ticketDate = currentDay != null ? currentDay.levelDate : "04/10/25";
        
        // 如果有自定义日期，使用自定义日期
        if (!string.IsNullOrEmpty(config.customShowDate))
        {
            ticketDate = config.customShowDate;
        }
        
        // 格式化日期
        string formattedDate = FormatDateToEnglish(ticketDate);

        var ticket = new TicketData
        {
            filmTitle = string.IsNullOrEmpty(config.customFilmTitle) ? show.filmTitle : config.customFilmTitle,
            showTime = string.IsNullOrEmpty(config.customShowTime) ? show.startTime : config.customShowTime,
            showDate = formattedDate, // 设置格式化后的日期
            special = config.type,
            hasStub = config.type != SpecialEventType.MissingStub, // 缺失票根没有票根
            isValid = config.shouldAccept // 使用配置的是否应该接受
        };
        
        return ticket;
    }

    /// <summary>
    /// 将 MM/dd/yy 格式的日期转换为 EnglishMonth/yy/dd 格式
    /// 例如：04/10/25 → October/25/10
    /// </summary>
    private string FormatDateToEnglish(string date)
    {
        if (string.IsNullOrEmpty(date))
            return date;
            
        try
        {
            // 分割日期部分
            string[] parts = date.Split('/');
            if (parts.Length == 3)
            {
                string month = parts[0];
                string day = parts[1];
                string year = parts[2];
                
                // 转换月份
                if (monthMapping.ContainsKey(month))
                {
                    return $"{monthMapping[month]}/{year}/{day}";
                }
                else
                {
                    Debug.LogWarning($"未知的月份: {month}，使用原始日期");
                    return date;
                }
            }
            else
            {
                Debug.LogWarning($"日期格式不正确: {date}，期望格式: MM/dd/yy");
                return date;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"日期转换错误: {date}, 错误: {e.Message}");
            return date;
        }
    }

    private Queue<TicketData> ShuffleQueue(Queue<TicketData> queue)
    {
        var list = ShuffleList(new List<TicketData>(queue));
        return new Queue<TicketData>(list);
    }
    
    private List<TicketData> ShuffleList(List<TicketData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            var temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
        return list;
    }

    public DaySchedule GetCurrentLevel()
    {
        if (database == null || database.levels == null || database.levels.Length == 0) return null;
        
        int clampedIndex = Mathf.Clamp(currentLevelIndex, 0, database.levels.Length - 1);

        if (clampedIndex < database.levels.Length)
        {
            var level = database.levels[clampedIndex];
            if (level != null)
            {
                return level;
            }
        }
    
        return null;
    }
    
    /// <summary>
    /// 获取接下来20分钟内的所有场次
    /// </summary>
    public List<DaySchedule.Show> GetUpcomingShows(ScheduleClock clock, int minutesAhead = 20)
    {
        var upcomingShows = new List<DaySchedule.Show>();
        var currentDay = GetCurrentLevel();
        
        if (currentDay == null) return upcomingShows;
        
        try
        {
            var currentTime = System.DateTime.ParseExact(clock.GetCurrentGameTime(), "HH:mm", System.Globalization.CultureInfo.InvariantCulture);
            
            foreach (var show in currentDay.shows)
            {
                var showTime = System.DateTime.ParseExact(show.startTime, "HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                double minutesUntilShow = (showTime - currentTime).TotalMinutes;
                
                // 只包括未来20分钟内的场次（不包括已开始的）
                if (minutesUntilShow > 0 && minutesUntilShow <= minutesAhead)
                {
                    upcomingShows.Add(show);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[TicketGenerator] 获取即将开始场次时出错: {e.Message}");
        }
        
        return upcomingShows;
    }
    
    public string GetCurrentLevelName()
    {
        var currentDay = GetCurrentLevel();
        return currentDay != null ? currentDay.levelName : "unknown";
    }

    public bool SetLevel(int index)
    {
        if (database == null || database.levels == null)
        {
            Debug.LogWarning("[TicketGenerator] 数据库为空，无法设置关卡");
            return false;
        }
    
        if (index < 0 || index >= database.levels.Length)
        {
            Debug.LogWarning($"[TicketGenerator] 关卡索引 {index} 超出范围，总关卡数: {database.levels.Length}");
            return false;
        }
    
        currentLevelIndex = index;
        var level = GetCurrentLevel();
    
        if (level == null)
        {
            Debug.LogError($"[TicketGenerator] 设置关卡失败，索引 {index} 对应的关卡为 null");
            return false;
        }
    
        Debug.Log($"[TicketGenerator] 成功设置关卡: {index}, 关卡名称: {level.levelName}");
        return true;
    }
}