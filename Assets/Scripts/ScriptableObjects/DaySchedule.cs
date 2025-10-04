using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DaySchedule", menuName = "Game/Schedule/DaySchedule")]
public class DaySchedule : ScriptableObject
{
    [Serializable]
    public class SpecialEventConfig
    {
        public SpecialEventType type;
        public int count;
        public string customFilmTitle;
        public string customShowTime;
        public bool shouldAccept;
        public string targetFilmTitle; // 目标电影（用于提前检票到其他场次）
        public string targetShowTime;   // 目标时间（用于提前检票到其他场次）
    }

    [Serializable]
    public class Show
    {
        [Header("基本信息")]
        public string filmTitle;
        public string startTime;
        [Range(0, 200)] public int audienceCount = 0;
        
        [Header("票价设置")]
        [Tooltip("本场次单张票的价格")]
        [Range(1, 20)] public int ticketPrice = 1;

        [Header("特殊事件配置")]
        public List<SpecialEventConfig> specialEvents = new List<SpecialEventConfig>();
    }
    
    [Header("场次安排")]
    public List<Show> shows = new List<Show>();

    [Header("时间流速设置")]
    [Tooltip("游戏时间流速倍率")]
    public float timeScale = 1f;
    
    [Header("时间间隔设置")]
    [Tooltip("场次之间的间隔时间（秒）")]
    public float timeBetweenShows = 2f;
    
    [Tooltip("票与票之间的间隔时间（秒）")]
    public float timeBetweenTickets = 0.5f;
    
    [Tooltip("票滑入动画持续时间（秒）")]
    public float ticketSlideInDuration = 0.5f;
    
    [Tooltip("票滑出动画持续时间（秒）")]
    public float ticketSlideOutDuration = 0.5f;
    
    [Tooltip("第一张票生成的初始延迟（秒）")]
    public float initialTicketDelay = 1f;

    [Header("评分标准")]
    public int star1Income = 20;
    public int star2Income = 50;
    public int star3Income = 90;
}