using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DaySchedule", menuName = "Game/Schedule/DaySchedule")]
public class DaySchedule : ScriptableObject
{
    [Serializable]
    public class Show
    {
        public string filmTitle;
        public string startTime; // "HH:mm"
        [Range(0, 200)] public int audienceCount = 0;

        [Range(0, 10)] public int special_Early = 0;
        [Range(0, 10)] public int special_OldTicket = 0;
        [Range(0, 10)] public int special_Copy = 0;
        [Range(0, 10)] public int special_Drawn = 0;
        [Range(0, 10)] public int special_Electronic = 0;
        [Range(0, 10)] public int special_Damaged = 0;
        [Range(0, 10)] public int special_WrongName = 0;
        [Range(0, 10)] public int special_MissingStub = 0;
    }

    public List<Show> shows = new List<Show>();
    public int star1Income = 20;
    public int star2Income = 50;
    public int star3Income = 90;
}
