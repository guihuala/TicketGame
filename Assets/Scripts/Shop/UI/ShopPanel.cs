using UnityEngine;
using UnityEngine.UI;

public class ShopPanel : BasePanel
{
    [SerializeField] private ShopItem[] shopItems;
    [SerializeField] private GameObject shopItemPrefab;
    [SerializeField] private Transform shopItemsContainer;
    [SerializeField] private Text coinsText;
    [SerializeField] private Button closeButton;
    
    private void Start()
    {
        closeButton.onClick.AddListener(CloseBtnClicked);
        RefreshShopUI();
        
        // 注册金币变化事件
        // MsgCenter.AddListener(MsgConst.MSG_COINS_CHANGED, OnCoinsChanged);
    }
    
    private void RefreshShopUI()
    {
        // 更新金币显示
        coinsText.text = CurrencyManager.GetCoins().ToString();
        
        // 清空容器
        foreach (Transform child in shopItemsContainer)
        {
            Destroy(child.gameObject);
        }
        
        // 生成商店物品
        foreach (var item in shopItems)
        {
            CreateShopItemButton(item);
        }
    }
    
    private void CreateShopItemButton(ShopItem item)
    {
        GameObject itemUI = Instantiate(shopItemPrefab, shopItemsContainer);
        ShopItemButton itemButton = itemUI.GetComponent<ShopItemButton>();
        
        if (itemButton != null)
        {
            itemButton.Initialize(item, OnPurchaseItem);
        }
        else
        {
            Debug.LogError("ShopItemPrefab 缺少 ShopItemButton 组件！");
            // 回退到旧方法
            SetupShopItemUIFallback(itemUI, item);
        }
    }
    
    // 回退方法（保持兼容性）
    private void SetupShopItemUIFallback(GameObject itemUI, ShopItem item)
    {
        // 原有的SetupShopItemUI逻辑...
        Image icon = itemUI.transform.Find("Icon").GetComponent<Image>();
        Text nameText = itemUI.transform.Find("Name").GetComponent<Text>();
        Text priceText = itemUI.transform.Find("Price").GetComponent<Text>();
        Text descriptionText = itemUI.transform.Find("Description").GetComponent<Text>();
        Button buyButton = itemUI.transform.Find("BuyButton").GetComponent<Button>();
        Text buyButtonText = buyButton.GetComponentInChildren<Text>();
        Text usesText = itemUI.transform.Find("UsesText").GetComponent<Text>();
        
        icon.sprite = item.icon;
        nameText.text = item.itemName;
        priceText.text = item.price.ToString();
        descriptionText.text = item.description;
        
        if (item.type == ShopItem.ItemType.Consumable)
        {
            int uses = ShopManager.GetItemUses(item.itemId);
            usesText.text = $"{uses}";
            usesText.gameObject.SetActive(true);
            buyButton.gameObject.SetActive(true);
            buyButtonText.text = "buy";
        }
        else
        {
            bool isPurchased = ShopManager.IsItemPurchased(item.itemId);
            buyButton.gameObject.SetActive(!isPurchased);
            usesText.gameObject.SetActive(false);
        }
        
        if (buyButton.gameObject.activeSelf)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => OnPurchaseItem(item));
            buyButton.interactable = CurrencyManager.GetCoins() >= item.price;
        }
    }

    private void OnPurchaseItem(ShopItem item)
    {
        if (ShopManager.PurchaseItem(item))
        {
            // 购买成功，刷新整个UI
            RefreshShopUI();
            AudioManager.Instance.PlaySfx("Purchase");
        }
        else
        {
            AudioManager.Instance.PlaySfx("Wrong");
        }
    }
    
    // 金币变化时的回调
    private void OnCoinsChanged()
    {
        coinsText.text = CurrencyManager.GetCoins().ToString();
        RefreshShopUI(); // 或者只更新按钮状态
    }
    
    private void CloseBtnClicked()
    {
        UIManager.Instance.ClosePanel(panelName);
    }
}