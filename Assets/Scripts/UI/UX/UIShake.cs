using UnityEngine;
using DG.Tweening;

public class UIShake : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform canvasRectTransform; // 需要震动的Canvas
    
    private Vector3 originalCanvasPosition;

    void Awake()
    {
        // 如果没有指定Canvas，自动查找主Canvas
        if (canvasRectTransform == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                canvasRectTransform = canvas.GetComponent<RectTransform>();
            }
        }
        
        if (canvasRectTransform != null)
        {
            originalCanvasPosition = canvasRectTransform.anchoredPosition3D;
        }
        
        // 注册相机震动消息监听
        MsgCenter.RegisterMsg(MsgConst.MSG_CAMERA_SHAKE, OnUIShakeRequested);
    }

    void OnDestroy()
    {
        // 注销消息监听
        MsgCenter.UnregisterMsg(MsgConst.MSG_CAMERA_SHAKE, OnUIShakeRequested);
    }

    /// <summary>
    /// 处理UI震动请求
    /// </summary>
    private void OnUIShakeRequested(params object[] parameters)
    {
        if (canvasRectTransform == null) return;
        
        if (parameters.Length > 0 && parameters[0] is TicketValidator.ShakeType shakeType)
        {
            switch (shakeType)
            {
                case TicketValidator.ShakeType.Light:
                    LightShake();
                    break;
                case TicketValidator.ShakeType.Medium:
                    MediumShake();
                    break;
                case TicketValidator.ShakeType.Heavy:
                    HeavyShake();
                    break;
            }
        }
    }

    // 轻微震动
    public void LightShake()
    {
        Shake(0.3f, 5f);
    }

    // 中等震动
    public void MediumShake()
    {
        Shake(0.5f, 10f);
    }

    // 强烈震动
    public void HeavyShake()
    {
        Shake(0.7f, 15f);
    }

    private void Shake(float duration, float strength)
    {
        if (canvasRectTransform == null) return;
        
        // 重置Canvas位置
        canvasRectTransform.anchoredPosition3D = originalCanvasPosition;
        
        // 使用DOTween震动UI Canvas
        canvasRectTransform.DOShakeAnchorPos(duration, strength, 20, 90, false, false)
            .OnComplete(() => {
                canvasRectTransform.anchoredPosition3D = originalCanvasPosition;
            });
    }
}