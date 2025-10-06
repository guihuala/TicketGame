using System;
using System.Globalization;
using UnityEngine;

public class ScheduleClock : MonoBehaviour
{
    private string currentFilm;
    private string currentShowTime; // "HH:mm"
    public float simSeconds;
    private bool finishedBeforeShowtime = true;
    private float levelStartTimeSeconds = 0f;

    public void SetLevelStartTime(string startTime)
    {
        if (DateTime.TryParseExact(startTime, "HH:mm",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateTime time))
        {
            levelStartTimeSeconds = (float)(time.TimeOfDay.TotalSeconds);
            simSeconds = levelStartTimeSeconds; // 设置初始时间
        }
        else
        {
            levelStartTimeSeconds = 8 * 3600f; // 默认8:00
            simSeconds = levelStartTimeSeconds;
        }
    }

    public void SetTargetShow(string film, string show)
    {
        currentFilm = film;
        currentShowTime = show;
        finishedBeforeShowtime = true;
        MsgCenter.SendMsg(MsgConst.MSG_SCHEDULE_SET, film, show);
    }

    void Update()
    {
        simSeconds += TimeManager.Instance.DeltaTime;
    }

    public bool IsFilmToday(string film) => !string.IsNullOrEmpty(currentFilm) && currentFilm == film;
    public bool IsCorrectShowtime(string show) => !string.IsNullOrEmpty(currentShowTime) && currentShowTime == show;
    
    public bool IsEarlierThanMinutes(string showHHmm, int minutes)
    {
        var now = SecondsToTime(simSeconds);
        var show = DateTime.ParseExact(showHHmm, "HH:mm", CultureInfo.InvariantCulture);
    
        // 计算距离开场还有多少分钟
        double minutesUntilShow = (show - now).TotalMinutes;
    
        // 如果距离开场还有20分钟或更多，就属于提前检票
        return minutesUntilShow >= minutes;
    }
    
    public bool AllProcessedBeforeShowtime() => finishedBeforeShowtime;

    private DateTime SecondsToTime(float s)
    {
        var midnight = DateTime.Today;
        return midnight.AddSeconds(s);
    }
    
    public string GetCurrentGameTime()
    {
        return SecondsToTime(simSeconds).ToString("HH:mm");
    }
    
    public string CurrentFilm => currentFilm;
    public string CurrentShowTime => currentShowTime;
}
