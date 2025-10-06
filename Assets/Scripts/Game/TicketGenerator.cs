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
        var list = new List<TicketData>(queue);
        for (int i = 0; i < list.Count; i++)
        {
            var temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
        return new Queue<TicketData>(list);
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
                Debug.Log($"[TicketGenerator] 加载关卡: {level.levelName}, 开始时间: {level.levelStartTime}，索引{currentLevelIndex}");
                return level;
            }
        }
    
        return null;
    }
    
    public string GetCurrentLevelName()
    {
        var currentDay = GetCurrentLevel();
        return currentDay != null ? currentDay.levelName : "unknown";
    }

    public void SetLevel(int index) => currentLevelIndex = index;
    public void SetDatabase(LevelDatabase db) => database = db;
}