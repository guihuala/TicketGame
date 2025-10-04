using UnityEngine;

public class TearInteraction : MonoBehaviour
{
    [SerializeField] private float tearDistance = 60f;   // 撕票的距离
    [SerializeField] private float minHoldMs = 200f;     // 最小按住时间（毫秒），从2ms增加到200ms更合理
    [SerializeField] private RectTransform tearArea;     // 票根可撕区域

    private bool holding;
    private Vector2 startPos;
    private float holdMillis;
    private bool isValidStart;  // 起始位置是否有效

    [SerializeField] private TicketVisual ticketVisual;

    public void Prepare()
    {
        holding = false;
        holdMillis = 0;
        isValidStart = false;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 clickPos = Input.mousePosition;
            
            // 检查点击位置是否在票根区域内
            if (IsPointInTearArea(clickPos))
            {
                holding = true;
                isValidStart = true;
                startPos = clickPos;
                holdMillis = 0;
            }
            else
            {
                holding = true;
                isValidStart = false;  // 起始位置不在票根区域
                startPos = clickPos;
                holdMillis = 0;
            }
        }
        
        if (holding)
        {
            holdMillis += Time.unscaledDeltaTime * 1000f;  // 累积按住时间
            
            // 只有起始位置有效时才检测撕票
            if (isValidStart && holdMillis >= minHoldMs)
            {
                Vector2 currentPos = Input.mousePosition;
                
                // 计算实际移动距离（不仅仅是垂直方向）
                float dist = Vector2.Distance(currentPos, startPos);
                
                // 检查移动方向 - 撕票通常是向下或斜向下移动
                Vector2 direction = (currentPos - startPos).normalized;
                bool isValidDirection = direction.y < -0.3f; // 主要向下移动（Y轴负方向）
                
                // 判断是否满足撕票条件：有效起始位置、足够距离、正确方向、足够时间
                if (dist >= tearDistance && isValidDirection)
                {
                    // 撕票成功后，给票卡添加视觉效果
                    ticketVisual.OnTearSuccess();
                    SendMessageUpwards("OnTearSucceeded", SendMessageOptions.DontRequireReceiver);
                    
                    holding = false;  // 成功后重置状态
                    isValidStart = false;
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                holding = false;  // 松开鼠标时停止拖动
                isValidStart = false;
            }
        }
    }
    
    // 检查点是否在票根可撕区域内
    private bool IsPointInTearArea(Vector2 screenPos)
    {
        if (tearArea == null) return true;  // 如果没有设置区域，默认允许
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(tearArea, screenPos, null, out Vector2 localPoint);
        return tearArea.rect.Contains(localPoint);
    }
}