using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DaySchedule", menuName = "Game/Schedule/DaySchedule")]
public class DaySchedule : ScriptableObject
{
    [Header("关卡基本信息")]
    [Tooltip("关卡名称（显示用）")]
    public string levelName = "04/10/25";
    
    [Header("关卡日期")]
    [Tooltip("关卡日期 (MM/dd/yy 格式，如 04/10/25)")]
    public string levelDate = "04/10/25";
    
    [Header("关卡开始时间")]
    [Tooltip("关卡开始的游戏时间 (HH:mm 格式，如 08:30、14:15)")]
    public string levelStartTime = "08:00";
    
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
    
    /// <summary>
    /// 验证开始时间格式是否正确
    /// </summary>
    public bool IsStartTimeValid()
    {
        return DateTime.TryParseExact(levelStartTime, "HH:mm", 
            System.Globalization.CultureInfo.InvariantCulture, 
            System.Globalization.DateTimeStyles.None, out _);
    }
    
    /// <summary>
    /// 验证日期格式是否正确
    /// </summary>
    public bool IsDateValid()
    {
        return DateTime.TryParseExact(levelDate, "MM/dd/yy", 
            System.Globalization.CultureInfo.InvariantCulture, 
            System.Globalization.DateTimeStyles.None, out _);
    }
    
    /// <summary>
    /// 获取开始时间的总秒数（从00:00开始计算）
    /// </summary>
    public float GetStartTimeInSeconds()
    {
        if (DateTime.TryParseExact(levelStartTime, "HH:mm", 
            System.Globalization.CultureInfo.InvariantCulture, 
            System.Globalization.DateTimeStyles.None, out DateTime time))
        {
            return (float)(time.TimeOfDay.TotalSeconds);
        }
        
        // 默认返回8:00
        return 8 * 3600f;
    }

    [Serializable]
    public class SpecialEventConfig
    {
        public SpecialEventType type;
        public int count;
        public string customFilmTitle;
        public string customShowTime;
        public string customShowDate;
        public bool shouldAccept;
        public string targetFilmTitle; // 目标电影（用于提前检票到其他场次）
        public string targetShowTime;   // 目标时间（用于提前检票到其他场次）
        public string targetShowDate;   // 目标日期（用于提前检票到其他场次）
        
        [Header("图片配置")]
        [Tooltip("主票替换图片")]
        public Sprite mainTicketImage;
        [Tooltip("票根替换图片")]
        public Sprite stubImage;
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
}