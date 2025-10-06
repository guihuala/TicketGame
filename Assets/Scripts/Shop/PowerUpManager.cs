using System;
using UnityEngine;
using System.Collections.Generic;

public class PowerUpManager : Singleton<PowerUpManager>
{
    private Dictionary<string, int> activePowerUps = new Dictionary<string, int>();
    private TicketQueueController ticketQueue;
    private ScheduleClock scheduleClock;
    
    public PowerUpUI powerUpUI;
    
    // 定义道具ID常量
    public const string UV_LIGHT_ID = "uv_light";
    public const string VIP_PASS_ID = "vip_pass";
    public const string BROADCAST_SYSTEM_ID = "broadcast_system";
    
    // 所有道具ID的数组，用于遍历
    private string[] allPowerUpIds = { UV_LIGHT_ID, VIP_PASS_ID, BROADCAST_SYSTEM_ID };

    protected override void Awake()
    {
        base.Awake();
        ticketQueue = FindObjectOfType<TicketQueueController>();
        scheduleClock = FindObjectOfType<ScheduleClock>();
        
        LoadPowerUpsFromPlayerPrefs();
    }
    

    private void LoadPowerUpsFromPlayerPrefs()
    {
        activePowerUps.Clear();
        
        // 加载所有道具的使用次数
        foreach (string itemId in allPowerUpIds)
        {
            int uses = ShopManager.GetItemUses(itemId);
            // 即使使用次数为0，也添加到字典中，避免KeyNotFoundException
            activePowerUps[itemId] = uses;
        }
        StartCoroutine(DelayedUpdateUI());
    }
    
    private System.Collections.IEnumerator DelayedUpdateUI()
    {
        yield return null; // 等待一帧
        
        if (powerUpUI != null)
        {
            powerUpUI.UpdatePowerUpUI();
        }
        else
        {
            Debug.LogWarning("[PowerUpManager] 找不到 PowerUpUI 组件");
        }
    }
    
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
            
            Debug.Log($"[PowerUpManager] 使用道具成功: {itemId}, 剩余: {GetPowerUpCount(itemId)}");
            
            // 立即更新UI
            UpdatePowerUpUI();
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
// 在 PowerUpManager.cs 中的 UseUVLight 方法修改为：
    private bool UseUVLight()
    {
        if (ticketQueue == null || !ticketQueue.IsWaitingForInput()) 
        {
            Debug.LogWarning("[UVLight] 没有待检票的票");
            return false;
        }
    
        AudioManager.Instance.PlaySfx("UV_Light");
        Debug.Log("[UVLight] 使用紫外线灯验证票的真伪");
    
        // 获取当前票并验证真伪
        bool isTicketValid = ticketQueue.ValidateCurrentTicketWithUVLight();
    
        // 显示真伪结果
        ticketQueue.ShowUVLightResult(isTicketValid);
    
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
        if (activePowerUps.ContainsKey(itemId))
        {
            return activePowerUps[itemId];
        }
        else
        {
            // 如果字典中没有这个key，说明该道具数量为0
            Debug.LogWarning($"[PowerUpManager] 道具 {itemId} 不在字典中，返回0");
            return 0;
        }
    }
    
    // 检查道具是否可用
    public bool IsPowerUpAvailable(string itemId)
    {
        return GetPowerUpCount(itemId) > 0;
    }
    
    public void RefreshPowerUpData()
    {
        LoadPowerUpsFromPlayerPrefs();
    }
    
    // 手动更新UI的方法
    public void UpdatePowerUpUI()
    {
        PowerUpUI ui = FindObjectOfType<PowerUpUI>();
        if (ui != null)
        {
            ui.UpdatePowerUpUI();
        }
    }
}