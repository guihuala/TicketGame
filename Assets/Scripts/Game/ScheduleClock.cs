using System;
using System.Globalization;
using UnityEngine;

public class ScheduleClock : MonoBehaviour
{
    private string currentFilm;
    private string currentShowTime; // "HH:mm"
    public float simSeconds;
    private bool finishedBeforeShowtime = true;

    public void SetTargetShow(string film, string show)
    {
        currentFilm = film;
        currentShowTime = show;
        finishedBeforeShowtime = true;
        MsgCenter.SendMsg(MsgConst.MSG_SCHEDULE_SET, film, show);
    }

    void Update()
    {
        // 使用 TimeManager 的 DeltaTime，这样在暂停时不会增加时间
        simSeconds += TimeManager.Instance.DeltaTime;
    }

    public bool IsFilmToday(string film) => !string.IsNullOrEmpty(currentFilm) && currentFilm == film;
    public bool IsCorrectShowtime(string show) => !string.IsNullOrEmpty(currentShowTime) && currentShowTime == show;

    public bool IsEarlierThanMinutes(string showHHmm, int minutes)
    {
        var now = SecondsToTime(simSeconds);
        var show = DateTime.ParseExact(showHHmm, "HH:mm", System.Globalization.CultureInfo.InvariantCulture);
        return (show - now).TotalMinutes >= minutes;
    }

    public void MarkFailedTiming() => finishedBeforeShowtime = false;
    public bool AllProcessedBeforeShowtime() => finishedBeforeShowtime;

    private DateTime SecondsToTime(float s)
    {
        var midnight = DateTime.Today;
        return midnight.AddSeconds(s);
    }

    // 获取当前游戏时间（用于UI显示等）
    public DateTime GetCurrentGameTime()
    {
        return SecondsToTime(simSeconds);
    }

    // 获取从开始到现在经过的游戏时间（小时）
    public float GetElapsedGameHours()
    {
        return simSeconds / 3600f;
    }
}
