using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PowerUpUI : MonoBehaviour
{
    [System.Serializable]
    public class PowerUpSlot
    {
        public string itemId;
        public Button button;
        public Text countText;
        public Text nameText;
        public GameObject slotContainer;
    }
    
    [SerializeField] private PowerUpSlot[] powerUpSlots;
    
    private void Awake()
    {
        foreach (var slot in powerUpSlots)
        {
            if (slot.button != null)
            {
                slot.button.onClick.AddListener(() => OnPowerUpClicked(slot.itemId));
            }
        }
    }
    
    public void UpdatePowerUpUI()
    {
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
                
                // 如果没有单独的容器，直接控制按钮的显示
                if (slot.slotContainer == null)
                {
                    slot.button.gameObject.SetActive(hasPowerUp);
                }
            }
            
            // 更新整个槽位的显示状态
            if (slot.slotContainer != null)
            {
                slot.slotContainer.SetActive(hasPowerUp);
            }
            else
            {
                // 如果没有单独的容器，隐藏相关文本
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
            else if (slot.button != null)
            {
                // 如果没有 CanvasGroup，直接设置颜色
                var colors = slot.button.colors;
                colors.normalColor = hasPowerUp ? Color.white : new Color(1, 1, 1, 0.3f);
                slot.button.colors = colors;
            }
        }
    }
    
    private void OnPowerUpClicked(string itemId)
    {
        if (PowerUpManager.Instance == null) return;
        
        Debug.Log($"[PowerUpUI] 点击道具: {itemId}");
        
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
            Debug.LogWarning($"[PowerUpUI] 使用道具失败: {itemId}");
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
}