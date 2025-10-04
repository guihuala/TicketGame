using UnityEngine;

public class TimeManager : Singleton<TimeManager>
{
    // 当前时间倍率
    public float TimeFactor { get; private set; } = 1f;

    // 用于处理暂停、恢复等功能的状态
    public bool IsPaused { get; private set; }

    // 时间倍率常数 (24:1 = 游戏内1小时=现实2.5分钟)
    private const float REAL_TIME_FACTOR = 24f; // 24倍速
    private const float REAL_SECONDS_PER_GAME_HOUR = 150f; // 2.5分钟 = 150秒

    public float DeltaTime
    {
        get
        {
            // 如果暂停，返回0，否则按时间倍率调整
            return IsPaused ? 0f : Time.unscaledDeltaTime * TimeFactor * REAL_TIME_FACTOR;
        }
    }

    // 获取固定更新的时间
    public float FixedDeltaTime
    {
        get
        {
            return IsPaused ? 0f : Time.fixedUnscaledDeltaTime * TimeFactor * REAL_TIME_FACTOR;
        }
    }

    // 设置时间倍率
    public void SetTimeFactor(float factor)
    {
        TimeFactor = Mathf.Clamp(factor, 0f, 10f); // 限制在0-10倍之间
        ApplyTimeScale();
    }

    // 暂停时间
    public void PauseTime()
    {
        IsPaused = true;
        ApplyTimeScale();
    }

    // 恢复时间
    public void ResumeTime()
    {
        IsPaused = false;
        ApplyTimeScale();
    }

    // 应用时间缩放
    private void ApplyTimeScale()
    {
        if (IsPaused)
        {
            Time.timeScale = 0f;
            Time.fixedDeltaTime = 0f;
        }
        else
        {
            Time.timeScale = TimeFactor;
            Time.fixedDeltaTime = 0.02f * TimeFactor;
        }
    }
}