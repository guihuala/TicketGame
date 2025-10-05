using UnityEngine;

public static class CurrencyManager
{
    private const string COINS_KEY = "PlayerCoins";
    
    // 获取当前货币数量
    public static int GetCoins()
    {
        return PlayerPrefs.GetInt(COINS_KEY, 0);
    }
    
    // 添加货币
    public static void AddCoins(int amount)
    {
        int currentCoins = GetCoins();
        PlayerPrefs.SetInt(COINS_KEY, currentCoins + amount);
        PlayerPrefs.Save();
        
        Debug.Log($"[CurrencyManager] 获得 {amount} 金币，当前总数: {GetCoins()}");
    }
    
    // 消费货币（如果货币足够）
    public static bool SpendCoins(int amount)
    {
        int currentCoins = GetCoins();
        if (currentCoins >= amount)
        {
            PlayerPrefs.SetInt(COINS_KEY, currentCoins - amount);
            PlayerPrefs.Save();
            Debug.Log($"[CurrencyManager] 消费 {amount} 金币，剩余: {GetCoins()}");
            return true;
        }
        return false;
    }
}