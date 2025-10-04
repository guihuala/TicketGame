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
        { "The Fast and Curious", new string[] { "The Fast and the Curious", "Fast and Curious", "The Fast & Curious" } }
    };

    public Queue<TicketData> BuildQueueForShow(DaySchedule.Show show)
    {
        var q = new Queue<TicketData>();
        var specialTickets = new List<TicketData>();

        // 创建特殊票
        specialTickets.AddRange(CreateSpecialTickets(SpecialEventType.EarlyCheck, show.special_Early, show));
        specialTickets.AddRange(CreateSpecialTickets(SpecialEventType.OldTicket, show.special_OldTicket, show));
        specialTickets.AddRange(CreateSpecialTickets(SpecialEventType.CopyTicket, show.special_Copy, show));
        specialTickets.AddRange(CreateSpecialTickets(SpecialEventType.DrawnTicket, show.special_Drawn, show));
        specialTickets.AddRange(CreateSpecialTickets(SpecialEventType.ElectronicAbuse, show.special_Electronic, show));
        specialTickets.AddRange(CreateSpecialTickets(SpecialEventType.DamagedTicket, show.special_Damaged, show));
        specialTickets.AddRange(CreateSpecialTickets(SpecialEventType.WrongNameSpelling, show.special_WrongName, show));
        specialTickets.AddRange(CreateSpecialTickets(SpecialEventType.MissingStub, show.special_MissingStub, show));

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

    private TicketData CreateNormalTicket(DaySchedule.Show show)
    {
        return new TicketData
        {
            filmTitle = show.filmTitle,
            showTime = show.startTime,
            special = SpecialEventType.None,
            hasStub = true,
            isValid = true
        };
    }

    private List<TicketData> CreateSpecialTickets(SpecialEventType type, int count, DaySchedule.Show show)
    {
        var list = new List<TicketData>();
        for (int i = 0; i < count; i++)
        {
            var ticket = CreateSpecialTicket(type, show);
            list.Add(ticket);
        }
        return list;
    }

    private TicketData CreateSpecialTicket(SpecialEventType type, DaySchedule.Show show)
    {
        var ticket = new TicketData
        {
            filmTitle = show.filmTitle,
            showTime = show.startTime,
            special = type,
            hasStub = true, // 默认有票根
            isValid = false // 特殊票默认无效
        };

        switch (type)
        {
            case SpecialEventType.EarlyCheck:
                // 提前检票：信息完全正确，只是来得太早
                ticket.isValid = true; // 票本身是有效的
                break;
                
            case SpecialEventType.OldTicket:
                // 旧影票：使用过去场次的票
                ticket.showTime = GeneratePastTimeForFilm(show.filmTitle);
                break;
                
            case SpecialEventType.WrongNameSpelling:
                // 错误命名：电影名拼写错误
                ticket.filmTitle = GetWrongSpelling(show.filmTitle);
                break;
                
            case SpecialEventType.DamagedTicket:
                // 受损影票：信息正确但票面有污渍
                ticket.isValid = true; // 应该放行
                break;
                
            case SpecialEventType.MissingStub:
                // 缺失票根：信息正确但没有票根
                ticket.hasStub = false;
                break;
                
            case SpecialEventType.DrawnTicket:
                // 手画票：信息可能正确但票是画的
                break;
                
            case SpecialEventType.CopyTicket:
                // 复制票：信息正确但是复印件
                break;
                
            case SpecialEventType.ElectronicAbuse:
                // 电子票滥用：信息正确但是截图
                break;
        }

        return ticket;
    }

    private string GeneratePastTimeForFilm(string filmTitle)
    {
        // 为不同电影生成不同的过去时间
        var timeMappings = new Dictionary<string, string[]>
        {
            { "Avocadar", new string[] { "06:00", "07:30", "08:15" } },
            { "Turning Green", new string[] { "08:00", "09:00" } },
            { "La La Lamb", new string[] { "10:00", "11:00" } },
            // 添加其他电影的过去时间
        };

        if (timeMappings.ContainsKey(filmTitle) && timeMappings[filmTitle].Length > 0)
        {
            return timeMappings[filmTitle][Random.Range(0, timeMappings[filmTitle].Length)];
        }

        // 默认生成比当前时间早1-3小时的时间
        var currentTime = System.DateTime.ParseExact("12:00", "HH:mm", null); // 示例
        var pastTime = currentTime.AddHours(-Random.Range(1, 4));
        return pastTime.ToString("HH:mm");
    }

    private string GetWrongSpelling(string correctName)
    {
        if (filmTypos.ContainsKey(correctName) && filmTypos[correctName].Length > 0)
        {
            return filmTypos[correctName][Random.Range(0, filmTypos[correctName].Length)];
        }
        return correctName; // 如果没有配置错误拼写，返回原名
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