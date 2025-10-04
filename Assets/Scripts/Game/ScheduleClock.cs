using System;
using System.Globalization;
using UnityEngine;

public class ScheduleClock : MonoBehaviour
{
    private string currentFilm;
    private string currentShowTime; // "HH:mm"
    public float simSeconds;
    private bool finishedBeforeShowtime = true;
    
    public string CurrentFilm => currentFilm;
    public string CurrentShowTime => currentShowTime;

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
}
