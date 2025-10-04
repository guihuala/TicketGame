using System.Collections.Generic;
using UnityEngine;

public class TicketGenerator : MonoBehaviour
{
    [SerializeField] private LevelDatabase database;
    [SerializeField] private int currentLevelIndex = 0;

    public Queue<TicketData> BuildQueueForShow(DaySchedule.Show show)
    {
        var q = new Queue<TicketData>();
        var pool = new List<SpecialEventType>();

        pool.AddRange(CreatePool(SpecialEventType.EarlyCheck, show.special_Early));
        pool.AddRange(CreatePool(SpecialEventType.OldTicket, show.special_OldTicket));
        pool.AddRange(CreatePool(SpecialEventType.CopyTicket, show.special_Copy));
        pool.AddRange(CreatePool(SpecialEventType.DrawnTicket, show.special_Drawn));
        pool.AddRange(CreatePool(SpecialEventType.ElectronicAbuse, show.special_Electronic));
        pool.AddRange(CreatePool(SpecialEventType.DamagedTicket, show.special_Damaged));

        for (int i = 0; i < show.audienceCount; i++)
        {
            var t = new TicketData
            {
                filmTitle = show.filmTitle,
                showTime = show.startTime,
                special = pool.Count > 0 ? pool[Random.Range(0, pool.Count)] : SpecialEventType.None,
                hasStub = true,
                isValid = true
            };
            q.Enqueue(t);
        }
        return q;
    }

    private List<SpecialEventType> CreatePool(SpecialEventType type, int count)
    {
        var list = new List<SpecialEventType>(count);
        for (int i = 0; i < count; i++) list.Add(type);
        return list;
    }

    public DaySchedule GetCurrentDay()
    {
        if (database == null || database.levels == null || database.levels.Length == 0) return null;
        return database.levels[Mathf.Clamp(currentLevelIndex, 0, database.levels.Length - 1)];
    }

    public void SetLevel(int index) => currentLevelIndex = index;
    public void SetDatabase(LevelDatabase db) => database = db;
}
