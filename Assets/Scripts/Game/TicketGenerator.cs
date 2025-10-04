using System.Collections.Generic;
using UnityEngine;

public class TicketGenerator : MonoBehaviour
{
    [SerializeField] private LevelDatabase database;
    [SerializeField] private int currentLevelIndex = 0;

    // 电影名拼写错误映射表
    private Dictionary<string, string[]> filmTypos = new Dictionary<string, string[]>
    {
        { "Turning Green", new string[] { "Turnlng Green", "Turnign Green", "Turning Gren" } },
        { "Avocadar", new string[] { "Avocado", "Avocada", "Avocador" } },
        { "La La Lamb", new string[] { "La La Land", "La La Lamp", "La Lamb" } },
        { "The Whale of Wall Street", new string[] { "The Whale on Wall Street", "The Whale Wall Street" } },
        { "The Legend of Hei", new string[] { "The Legend of He", "Legend of Hei", "The Legnd of Hei" } },
        { "Coco Nuts", new string[] { "Coconuts", "Coco Nut", "Coco Nuts" } },
        { "The Fast and Curious", new string[] { "The Fast and the Curious", "Fast and Curious", "The Fast & Curious" } },
        { "The Faults in Our Starbucks", new string[] { "The FauIts ln Our Starbucks", "A Starbuck is Born", "Faults in Our Starbucks" } },
        { "Nezha", new string[] { "Ne Zha", "Nezhaa", "Nezha!" } },
        { "Cat-anic", new string[] { "Catanic", "Cat Anic", "Cat-anic!" } }
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
        return new TicketData
        {
            filmTitle = show.filmTitle,
            showTime = show.startTime,
            special = SpecialEventType.None,
            hasStub = true,
            isValid = true  // 正常票默认有效
        };
    }

    private TicketData CreateSpecialTicket(DaySchedule.SpecialEventConfig config, DaySchedule.Show show)
    {
        var ticket = new TicketData
        {
            filmTitle = string.IsNullOrEmpty(config.customFilmTitle) ? show.filmTitle : config.customFilmTitle,
            showTime = string.IsNullOrEmpty(config.customShowTime) ? show.startTime : config.customShowTime,
            special = config.type,
            hasStub = config.type != SpecialEventType.MissingStub, // 缺失票根没有票根
            isValid = config.shouldAccept // 使用配置的是否应该接受
        };

        // 根据类型应用特定逻辑
        ApplySpecialTicketLogic(config.type, ref ticket, show);

        return ticket;
    }
    
    private void ApplySpecialTicketLogic(SpecialEventType type, ref TicketData ticket, DaySchedule.Show show)
    {
        switch (type)
        {
            case SpecialEventType.EarlyCheck:
                ticket.isValid = true;
                break;
                
            case SpecialEventType.OldTicket:
                ticket.showTime = GeneratePastTimeForFilm(show.filmTitle);
                break;
                
            case SpecialEventType.WrongNameSpelling:
                ticket.filmTitle = GetWrongSpelling(show.filmTitle);
                break;
                
            case SpecialEventType.DamagedTicket:
                ticket.isValid = true; // 受损票但信息正确，应该放行
                break;
                
            case SpecialEventType.MissingStub:
                ticket.hasStub = false;
                break;
        }
    }
    
    private string GeneratePastTimeForFilm(string filmTitle)
    {
        // 为不同电影生成不同的过去时间
        var timeMappings = new Dictionary<string, string[]>
        {
            { "Avocadar", new string[] { "06:00", "07:30", "08:15" } },
            { "Turning Green", new string[] { "08:00", "09:00" } },
            { "La La Lamb", new string[] { "10:00", "11:00", "13:55" } }, // 05/10/25 13:55
            { "The Whale of Wall Street", new string[] { "10:30", "11:45" } }
        };

        if (timeMappings.ContainsKey(filmTitle) && timeMappings[filmTitle].Length > 0)
        {
            return timeMappings[filmTitle][Random.Range(0, timeMappings[filmTitle].Length)];
        }

        var currentTime = System.DateTime.ParseExact("12:00", "HH:mm", null);
        var pastTime = currentTime.AddHours(-Random.Range(1, 4));
        return pastTime.ToString("HH:mm");
    }

    private string GetWrongSpelling(string correctName)
    {
        if (filmTypos.ContainsKey(correctName) && filmTypos[correctName].Length > 0)
        {
            return filmTypos[correctName][Random.Range(0, filmTypos[correctName].Length)];
        }
        return correctName;
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

    public DaySchedule GetCurrentDay()
    {
        if (database == null || database.levels == null || database.levels.Length == 0) return null;
        return database.levels[Mathf.Clamp(currentLevelIndex, 0, database.levels.Length - 1)];
    }

    public void SetLevel(int index) => currentLevelIndex = index;
    public void SetDatabase(LevelDatabase db) => database = db;
}