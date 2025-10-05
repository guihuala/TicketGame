using UnityEngine;

public static class ShopManager
{
    private const string PURCHASE_PREFIX = "Purchased_";
    private const string USES_PREFIX = "Uses_";
    
    // 购买物品
    public static bool PurchaseItem(ShopItem item)
    {
        if (CurrencyManager.SpendCoins(item.price))
        {
            if (item.type == ShopItem.ItemType.Consumable)
            {
                // 消耗品：增加使用次数（直接写入注册表）
                AddItemUses(item.itemId, item.uses);
            }
            else
            {
                // 永久物品：标记为已购买
                PlayerPrefs.SetInt($"{PURCHASE_PREFIX}{item.itemId}", 1);
            }
            
            PlayerPrefs.Save();
            Debug.Log($"[ShopManager] 成功购买: {item.itemName}");
            return true;
        }
        else
        {
            Debug.Log($"[ShopManager] 金币不足，无法购买: {item.itemName}");
            return false;
        }
    }
    
    // 获取物品使用次数
    public static int GetItemUses(string itemId)
    {
        return PlayerPrefs.GetInt($"{USES_PREFIX}{itemId}", 0);
    }
    
    // 增加物品使用次数
    private static void AddItemUses(string itemId, int amount)
    {
        int currentUses = GetItemUses(itemId);
        PlayerPrefs.SetInt($"{USES_PREFIX}{itemId}", currentUses + amount);
    }
    
    // 消耗物品使用次数
    public static bool ConsumeItemUse(string itemId)
    {
        int currentUses = GetItemUses(itemId);
        if (currentUses > 0)
        {
            PlayerPrefs.SetInt($"{USES_PREFIX}{itemId}", currentUses - 1);
            PlayerPrefs.Save();
            return true;
        }
        return false;
    }
    
    // 检查永久物品是否已购买
    public static bool IsItemPurchased(string itemId)
    {
        return PlayerPrefs.GetInt($"{PURCHASE_PREFIX}{itemId}", 0) == 1;
    }
}