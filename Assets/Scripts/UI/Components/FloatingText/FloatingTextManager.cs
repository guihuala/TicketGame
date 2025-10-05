using UnityEngine;

[System.Serializable]
public class FloatingTextColorConfig
{
    public Color positiveColor = Color.green;      // 正数颜色
    public Color negativeColor = Color.red;        // 负数颜色
}

public class FloatingTextManager : Singleton<FloatingTextManager>
{
    [SerializeField] private FloatingText floatingTextPrefab;
    [SerializeField] private Canvas canvas;

    [Header("颜色配置")]
    [SerializeField] private FloatingTextColorConfig colorConfig = new FloatingTextColorConfig();
    
    public void ShowFloatingText(string text, Color color, Vector3 worldPosition)
    {
        if (floatingTextPrefab == null || canvas == null) return;
        
        // 将世界坐标转换为相对于父物体的本地坐标
        Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
        
        // 实例化浮动文本，指定父物体，使用转换后的本地坐标
        FloatingText floatingText = Instantiate(floatingTextPrefab, localPosition, Quaternion.identity, transform);
        floatingText.Initialize(text, color, localPosition);
    }

    // 显示金钱变化
    public void ShowMoneyChange(int amount, Vector3 worldPosition)
    {
        if (amount == 0) return;

        string text = amount > 0 ? $"+${amount}" : $"-${Mathf.Abs(amount)}";
        Color color = amount > 0 ? colorConfig.positiveColor : colorConfig.negativeColor;

        ShowFloatingText(text, color, worldPosition);
    }
}