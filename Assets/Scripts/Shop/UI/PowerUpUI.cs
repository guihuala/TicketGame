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
        public GameObject slotContainer; // 整个槽位的容器，用于隐藏/显示
    }
    
    [SerializeField] private PowerUpSlot[] powerUpSlots;
    
    private void Start()
    {
        InitializePowerUpUI();
        
        foreach (var slot in powerUpSlots)
        {
            slot.button.onClick.AddListener(() => OnPowerUpClicked(slot.itemId));
        }
    }
    
    private void InitializePowerUpUI()
    {
        if (PowerUpManager.Instance == null) return;
        
        foreach (var slot in powerUpSlots)
        {
            // 设置图标
            Sprite iconSprite = PowerUpManager.Instance.GetPowerUpIcon(slot.itemId);
            if (iconSprite != null && slot.icon != null)
            {
                slot.icon.sprite = iconSprite;
                slot.icon.preserveAspect = true; // 保持图标比例
            }
            else if (slot.icon != null)
            {
                Debug.LogWarning($"[PowerUpUI] 无法为道具 {slot.itemId} 加载图标");
            }
            
            // 设置名称
            if (slot.nameText != null)
            {
                slot.nameText.text = PowerUpManager.Instance.GetPowerUpName(slot.itemId);
            }
        }
        
        UpdatePowerUpUI();
    }
    
    private void UpdatePowerUpUI()
    {
        if (PowerUpManager.Instance == null) return;
        
        foreach (var slot in powerUpSlots)
        {
            int count = PowerUpManager.Instance.GetPowerUpCount(slot.itemId);
            bool hasPowerUp = count > 0;
            
            // 更新数量显示
            if (slot.countText != null)
            {
                slot.countText.text = count.ToString();
                slot.countText.gameObject.SetActive(hasPowerUp);
            }
            
            // 更新按钮交互状态
            if (slot.button != null)
            {
                slot.button.interactable = hasPowerUp;
            }
            
            // 更新整个槽位的显示状态
            if (slot.slotContainer != null)
            {
                // 如果有单独的容器，隐藏整个容器
                slot.slotContainer.SetActive(hasPowerUp);
            }
            else
            {
                // 如果没有单独的容器，隐藏按钮
                slot.button.gameObject.SetActive(hasPowerUp);
                
                // 同时隐藏图标和文本
                if (slot.icon != null)
                    slot.icon.gameObject.SetActive(hasPowerUp);
                if (slot.nameText != null)
                    slot.nameText.gameObject.SetActive(hasPowerUp);
                if (slot.countText != null)
                    slot.countText.gameObject.SetActive(hasPowerUp);
            }
            
            // 视觉反馈 - 透明度
            CanvasGroup canvasGroup = slot.button.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = hasPowerUp ? 1f : 0.3f;
            }
            else if (slot.icon != null)
            {
                // 如果没有CanvasGroup，直接调整图标颜色
                Color iconColor = slot.icon.color;
                iconColor.a = hasPowerUp ? 1f : 0.3f;
                slot.icon.color = iconColor;
            }
            
            // 更新名称文本颜色
            if (slot.nameText != null)
            {
                slot.nameText.color = hasPowerUp ? Color.white : new Color(1f, 1f, 1f, 0.3f);
            }
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
            
            // 可以添加使用成功的视觉反馈
            StartCoroutine(PlayUseEffect(itemId));
        }
        else
        {
            AudioManager.Instance.PlaySfx("Wrong");
        }
    }
    
    // 使用道具的视觉反馈
    private System.Collections.IEnumerator PlayUseEffect(string itemId)
    {
        // 找到对应的槽位
        PowerUpSlot usedSlot = null;
        foreach (var slot in powerUpSlots)
        {
            if (slot.itemId == itemId)
            {
                usedSlot = slot;
                break;
            }
        }
        
        if (usedSlot != null && usedSlot.button != null)
        {
            // 简单的缩放动画
            Vector3 originalScale = usedSlot.button.transform.localScale;
            usedSlot.button.transform.localScale = originalScale * 0.8f;
            
            yield return new WaitForSeconds(0.1f);
            
            usedSlot.button.transform.localScale = originalScale;
        }
    }
    
    private void OnEnable()
    {
        // 重新加载数据并更新UI
        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.RefreshPowerUpData();
            UpdatePowerUpUI();
        }
    }
}