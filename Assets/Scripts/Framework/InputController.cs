using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class InputController : Singleton<InputController>
{
    public System.Action onAcceptTicket;
    public System.Action onRejectTicket;
    
    [SerializeField] private LayerMask ignoredUILayers = 1 << 5;
    
    private bool inputEnabled = true;
    
    void Update()
    {
        if (!inputEnabled) return;
        
        // 检查是否点击在需要忽略的UI元素上
        if (IsPointerOverIgnoredUI())
            return;
        
        if (Input.GetMouseButtonDown(0))
        {
            onAcceptTicket?.Invoke();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            onRejectTicket?.Invoke();
        }
    }
    
    // 检查鼠标是否在需要忽略的UI元素上
    private bool IsPointerOverIgnoredUI()
    {
        if (EventSystem.current == null)
            return false;
            
        // 创建PointerEventData
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        
        // 进行射线检测
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        
        // 检查是否有需要忽略的UI元素（基于层级）
        foreach (var result in results)
        {
            GameObject uiObject = result.gameObject;
            
            // 检查该UI是否在忽略的层级中
            if (IsInIgnoredLayer(uiObject))
            {
                return true;
            }
        }
        
        return false;
    }
    
    // 检查GameObject是否在忽略的层级中
    private bool IsInIgnoredLayer(GameObject obj)
    {
        // 使用位运算检查层级
        return ignoredUILayers == (ignoredUILayers | (1 << obj.layer));
    }
    
    public void EnableInput(bool enable)
    {
        inputEnabled = enable;
    }
}