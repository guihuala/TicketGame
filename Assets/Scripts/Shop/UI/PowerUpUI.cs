using UnityEngine;
using UnityEngine.UI;

public class PowerUpUI : MonoBehaviour
{
    [System.Serializable]
    public class PowerUpSlot
    {
        public string itemId;
        public Button button;
        public Text countText;
        public Image icon;
        public Text nameText;
    }
    
    [SerializeField] private PowerUpSlot[] powerUpSlots;
    
    private void Start()
    {
        UpdatePowerUpUI();
        
        foreach (var slot in powerUpSlots)
        {
            slot.button.onClick.AddListener(() => OnPowerUpClicked(slot.itemId));
        }
    }
    
    private void UpdatePowerUpUI()
    {
        if (PowerUpManager.Instance == null) return;
        
        foreach (var slot in powerUpSlots)
        {
            int count = PowerUpManager.Instance.GetPowerUpCount(slot.itemId);
            slot.countText.text = count.ToString();
            slot.button.interactable = count > 0;
            
            
            // 更新名称显示（可选）
            if (slot.nameText != null)
            {
                slot.nameText.text = GetPowerUpName(slot.itemId);
            }
            
            // 视觉反馈
            CanvasGroup canvasGroup = slot.button.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = count > 0 ? 1f : 0.5f;
            }
        }
    }
    
    private string GetPowerUpName(string itemId)
    {
        switch (itemId)
        {
            case PowerUpManager.UV_LIGHT_ID: return "紫外线灯";
            case PowerUpManager.VIP_PASS_ID: return "VIP票";
            case PowerUpManager.BROADCAST_SYSTEM_ID: return "广播系统";
            default: return "未知道具";
        }
    }
    
    private void OnPowerUpClicked(string itemId)
    {
        if (PowerUpManager.Instance == null) return;
        
        bool success = PowerUpManager.Instance.UsePowerUp(itemId);
        if (success)
        {
            UpdatePowerUpUI();
            AudioManager.Instance.PlaySfx("PowerUp_Use");
        }
        else
        {
            AudioManager.Instance.PlaySfx("Wrong");
        }
    }
    
    private void OnEnable()
    {
        UpdatePowerUpUI();
    }
}