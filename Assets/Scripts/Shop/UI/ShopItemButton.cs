using UnityEngine;
using UnityEngine.UI;

public class ShopItemButton : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image icon;
    [SerializeField] private Text nameText;
    [SerializeField] private Text priceText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Button buyButton;
    [SerializeField] private Text buyButtonText;
    [SerializeField] private Text usesText;
    
    private ShopItem currentItem;
    private System.Action<ShopItem> onPurchaseCallback;
    
    // 初始化商品按钮
    public void Initialize(ShopItem item, System.Action<ShopItem> purchaseCallback)
    {
        currentItem = item;
        onPurchaseCallback = purchaseCallback;
        
        UpdateUI();
        SetupButton();
    }
    
    // 更新UI显示
    public void UpdateUI()
    {
        if (currentItem == null) return;
        
        // 设置基础信息
        icon.sprite = currentItem.icon;
        nameText.text = currentItem.itemName;
        priceText.text = currentItem.price.ToString();
        descriptionText.text = currentItem.description;
        
        // 更新金币显示
        UpdateCoinsDisplay();
        
        // 根据物品类型设置UI
        if (currentItem.type == ShopItem.ItemType.Consumable)
        {
            SetupConsumableUI();
        }
        else
        {
            SetupPermanentUI();
        }
    }
    
    private void SetupConsumableUI()
    {
        int uses = ShopManager.GetItemUses(currentItem.itemId);
        usesText.text = $"{uses}";
        usesText.gameObject.SetActive(true);
        buyButton.gameObject.SetActive(true);
        buyButtonText.text = "buy";
        
        // 设置按钮交互状态
        buyButton.interactable = CurrencyManager.GetCoins() >= currentItem.price;
    }
    
    private void SetupPermanentUI()
    {
        bool isPurchased = ShopManager.IsItemPurchased(currentItem.itemId);
        buyButton.gameObject.SetActive(!isPurchased);
        usesText.gameObject.SetActive(false);
        
        if (buyButton.gameObject.activeSelf)
        {
            buyButton.interactable = CurrencyManager.GetCoins() >= currentItem.price;
        }
    }
    
    private void SetupButton()
    {
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyButtonClicked);
    }
    
    private void OnBuyButtonClicked()
    {
        if (currentItem == null) return;
        
        if (CurrencyManager.GetCoins() >= currentItem.price)
        {
            onPurchaseCallback?.Invoke(currentItem);
        }
        else
        {
            AudioManager.Instance.PlaySfx("Wrong");
            // 可以添加金币不足的视觉反馈
            StartCoroutine(ShakeButton());
        }
    }
    
    // 金币不足时的按钮抖动效果
    private System.Collections.IEnumerator ShakeButton()
    {
        Vector3 originalPosition = buyButton.transform.localPosition;
        float shakeDuration = 0.5f;
        float shakeMagnitude = 5f;
        float elapsed = 0f;
        
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;
            buyButton.transform.localPosition = originalPosition + new Vector3(x, y, 0);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        buyButton.transform.localPosition = originalPosition;
    }
    
    // 更新金币显示（可以响应金币变化事件）
    private void UpdateCoinsDisplay()
    {
        // 这里可以添加金币变化的视觉反馈
        bool canAfford = CurrencyManager.GetCoins() >= currentItem.price;
        
        // 根据能否购买改变价格文本颜色
        priceText.color = canAfford ? Color.white : Color.red;
    }
    
    public void OnCoinsChanged()
    {
        UpdateUI();
    }
    
    // 清理方法
    public void Cleanup()
    {
        buyButton.onClick.RemoveAllListeners();
        currentItem = null;
        onPurchaseCallback = null;
    }
}