using UnityEngine;

[CreateAssetMenu(fileName = "ShopItem", menuName = "Game/Shop/ShopItem")]
public class ShopItem : ScriptableObject
{
    public string itemId;
    public string itemName;
    [TextArea] public string description;
    public int price;
    public Sprite icon;
    public ItemType type = ItemType.Consumable;
    public int uses = 1; // 使用次数

    public enum ItemType
    {
        Consumable, // 消耗品（道具）
        Permanent // 永久物品
    }
}