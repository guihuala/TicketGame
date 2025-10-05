using UnityEngine;
using UnityEngine.UI;

public class ShopPanel : BasePanel
{
    [System.Serializable]
    public class ShopItemSlot
    {
        public ShopItem item;           // 商品数据
        public Button buyButton;        // 购买按钮
    }
    
    [SerializeField] private ShopItemSlot[] shopItemSlots; // 手动分配的商品槽位
    [SerializeField] private Text coinsText;
    [SerializeField] private Button closeButton;
    
    private void Start()
    {
        closeButton.onClick.AddListener(CloseBtnClicked);
        InitializeShopUI();
    }
    
    private void InitializeShopUI()
    {
        // 更新金币显示
        coinsText.text = CurrencyManager.GetCoins().ToString();
        
        // 初始化每个商品槽位
        foreach (var slot in shopItemSlots)
        {
            if (slot.item != null)
            {
                SetupShopItemSlot(slot);
            }
            else
            {
                Debug.LogWarning("发现未分配商品的商店槽位");
            }
        }
    }
    
    private void SetupShopItemSlot(ShopItemSlot slot)
    {
        // 设置按钮点击事件
        if (slot.buyButton != null)
        {
            slot.buyButton.onClick.RemoveAllListeners();
            slot.buyButton.onClick.AddListener(() => OnPurchaseItem(slot.item));
        }
        
        // 更新UI状态
        UpdateShopItemSlotUI(slot);
    }
    
    private void UpdateShopItemSlotUI(ShopItemSlot slot)
    {
        if (slot.item == null) return;
        
        // 更新金币显示
        coinsText.text = CurrencyManager.GetCoins().ToString();
    }

    private void OnPurchaseItem(ShopItem item)
    {
        if (ShopManager.PurchaseItem(item))
        {
            // 购买成功，更新UI
            RefreshShopUI();
            AudioManager.Instance.PlaySfx("Purchase");
        }
        else
        {
            AudioManager.Instance.PlaySfx("Wrong");
        }
    }
    
    private void RefreshShopUI()
    {
        // 更新所有商品槽位的UI状态
        foreach (var slot in shopItemSlots)
        {
            if (slot.item != null)
            {
                UpdateShopItemSlotUI(slot);
            }
        }
    }
    
    private void CloseBtnClicked()
    {
        UIManager.Instance.ClosePanel(panelName);
    }
    
    // 当面板激活时刷新
    private void OnEnable()
    {
        RefreshShopUI();
    }
}