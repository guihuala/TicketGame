using UnityEngine;

public static class LevelProgressManager
{
    private const string PROGRESS_KEY = "LevelProgress";
    private const string STARS_KEY_PREFIX = "LevelStars_";
    
    // 获取已解锁的最大关卡索引（从0开始）
    public static int GetMaxUnlockedLevel()
    {
        return PlayerPrefs.GetInt(PROGRESS_KEY, 0); // 默认解锁第一关
    }
    
    // 解锁新关卡
    public static void UnlockLevel(int levelIndex)
    {
        int currentMax = GetMaxUnlockedLevel();
        if (levelIndex > currentMax)
        {
            PlayerPrefs.SetInt(PROGRESS_KEY, levelIndex);
            PlayerPrefs.Save();
        }
    }
    
    // 检查关卡是否已解锁
    public static bool IsLevelUnlocked(int levelIndex)
    {
        return levelIndex <= GetMaxUnlockedLevel();
    }
    
    // 获取关卡的星星数量
    public static int GetLevelStars(int levelIndex)
    {
        return PlayerPrefs.GetInt($"{STARS_KEY_PREFIX}{levelIndex}", 0);
    }
    
    // 设置关卡的星星数量
    public static void SetLevelStars(int levelIndex, int stars)
    {
        int currentStars = GetLevelStars(levelIndex);
        if (stars > currentStars)
        {
            PlayerPrefs.SetInt($"{STARS_KEY_PREFIX}{levelIndex}", stars);
            PlayerPrefs.Save();
        }
    }
    
    // 完成关卡并解锁下一关
    public static void CompleteLevel(int levelIndex, int starsEarned)
    {
        SetLevelStars(levelIndex, starsEarned);
        
        // 解锁下一关
        if (levelIndex >= GetMaxUnlockedLevel())
        {
            UnlockLevel(levelIndex + 1);
        }
    }
    
    // 重置所有进度
    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(PROGRESS_KEY);
        
        // 删除所有星星记录
        for (int i = 0; i < 20; i++)
        {
            PlayerPrefs.DeleteKey($"{STARS_KEY_PREFIX}{i}");
        }
        
        PlayerPrefs.Save();
    }
    
    // 获取总星星数
    public static int GetTotalStars()
    {
        int total = 0;
        int maxLevel = GetMaxUnlockedLevel();
        
        for (int i = 0; i <= maxLevel; i++)
        {
            total += GetLevelStars(i);
        }
        
        return total;
    }
}