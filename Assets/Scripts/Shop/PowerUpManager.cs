using UnityEngine;
using System.Collections.Generic;

public class PowerUpManager : Singleton<PowerUpManager>
{
    private Dictionary<string, int> activePowerUps = new Dictionary<string, int>();
    private TicketQueueController ticketQueue;
    private ScheduleClock scheduleClock;
    
    // 定义道具ID常量
    public const string UV_LIGHT_ID = "uv_light";
    public const string VIP_PASS_ID = "vip_pass";
    public const string BROADCAST_SYSTEM_ID = "broadcast_system";
    
    protected override void Awake()
    {
        base.Awake();
        ticketQueue = FindObjectOfType<TicketQueueController>();
        scheduleClock = FindObjectOfType<ScheduleClock>();
        
        // 游戏开始时从注册表加载道具数据
        LoadPowerUpsFromPlayerPrefs();
    }
    
    // 从PlayerPrefs加载所有道具
    private void LoadPowerUpsFromPlayerPrefs()
    {
        activePowerUps.Clear();
        
        // 加载所有道具的使用次数
        string[] allPowerUpIds = { UV_LIGHT_ID, VIP_PASS_ID, BROADCAST_SYSTEM_ID };
        
        foreach (string itemId in allPowerUpIds)
        {
            int uses = ShopManager.GetItemUses(itemId);
            if (uses > 0)
            {
                activePowerUps[itemId] = uses;
                Debug.Log($"[PowerUpManager] 从注册表加载道具: {itemId} x{uses}");
            }
        }
        
        Debug.Log($"[PowerUpManager] 道具加载完成，总计 {activePowerUps.Count} 种道具");
    }
    
    // 使用道具
    public bool UsePowerUp(string itemId)
    {
        if (!activePowerUps.ContainsKey(itemId) || activePowerUps[itemId] <= 0)
        {
            Debug.Log($"[PowerUpManager] 道具不足或不存在: {itemId}");
            return false;
        }
        
        bool success = ExecutePowerUpEffect(itemId);
        
        if (success)
        {
            // 更新内存中的数量
            activePowerUps[itemId]--;
            
            // 更新注册表中的数量
            ShopManager.ConsumeItemUse(itemId);
            
            // 如果数量为0，从字典中移除
            if (activePowerUps[itemId] <= 0)
            {
                activePowerUps.Remove(itemId);
            }
            
            Debug.Log($"[PowerUpManager] 使用道具成功: {itemId}, 剩余: {GetPowerUpCount(itemId)}");
        }
        
        return success;
    }
    
    // 执行道具效果
    private bool ExecutePowerUpEffect(string itemId)
    {
        switch (itemId)
        {
            case UV_LIGHT_ID:
                return UseUVLight();
            case VIP_PASS_ID:
                return UseVIPPass();
            case BROADCAST_SYSTEM_ID:
                return UseBroadcastSystem();
            default:
                Debug.LogWarning($"[PowerUpManager] 未知的道具ID: {itemId}");
                return false;
        }
    }
    
    // 道具效果实现
    private bool UseUVLight()
    {
        if (ticketQueue == null || !ticketQueue.IsWaitingForInput()) 
        {
            Debug.LogWarning("[UVLight] 没有待检票的票");
            return false;
        }
        
        AudioManager.Instance.PlaySfx("UV_Light");
        Debug.Log("[UVLight] 使用紫外线灯验证票的真伪");
        
        // TODO: 在这里添加紫外线灯的具体效果
        // 比如显示票的真伪信息、高亮显示问题区域等
        
        return true;
    }
    
    private bool UseVIPPass()
    {
        if (ticketQueue == null || !ticketQueue.IsWaitingForInput()) 
        {
            Debug.LogWarning("[VIPPass] 没有待检票的票");
            return false;
        }
        
        AudioManager.Instance.PlaySfx("VIP_Pass");
        ticketQueue.AcceptCurrentTicket();
        Debug.Log("[VIPPass] 使用VIP票，观众直接通过");
        return true;
    }
    
    private bool UseBroadcastSystem()
    {
        if (scheduleClock == null) 
        {
            Debug.LogWarning("[BroadcastSystem] 找不到时钟系统");
            return false;
        }
        
        AudioManager.Instance.PlaySfx("Broadcast");
        scheduleClock.simSeconds -= 900f; // 时间倒退15分钟
        MsgCenter.SendMsg(MsgConst.MSG_BROADCAST_DELAY, 15);
        Debug.Log("[BroadcastSystem] 使用广播系统，电影延迟15分钟开场");
        return true;
    }
    
    // 获取道具数量
    public int GetPowerUpCount(string itemId)
    {
        return activePowerUps.ContainsKey(itemId) ? activePowerUps[itemId] : 0;
    }
    
    // 检查道具是否可用
    public bool IsPowerUpAvailable(string itemId)
    {
        return GetPowerUpCount(itemId) > 0;
    }

    // 获取道具名称
    public string GetPowerUpName(string itemId)
    {
        switch (itemId)
        {
            case UV_LIGHT_ID: return "紫外线灯";
            case VIP_PASS_ID: return "VIP票";
            case BROADCAST_SYSTEM_ID: return "广播系统";
            default: return "未知道具";
        }
    }
    
    public void RefreshPowerUpData()
    {
        LoadPowerUpsFromPlayerPrefs();
    }
}