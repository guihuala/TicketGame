using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class OperationFeedbackUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup feedbackCanvasGroup;
    [SerializeField] private Image feedbackImage;
    [SerializeField] private RectTransform feedbackRect;
    
    [Header("Feedback Sprites")]
    [SerializeField] private Sprite correctSprite;
    [SerializeField] private Sprite wrongSprite;
    
    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float displayDuration = 1.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float scaleInAmount = 1.2f;
    [SerializeField] private float scaleOutAmount = 0.8f;
    
    [Header("Position Settings")]
    [SerializeField] private Vector2 screenOffset = new Vector2(0, 100f);
    
    private Sequence currentAnimation;
    private Camera mainCamera;
    
    private void Awake()
    {
        // 确保初始状态是隐藏的
        if (feedbackCanvasGroup != null)
        {
            feedbackCanvasGroup.alpha = 0f;
            feedbackCanvasGroup.blocksRaycasts = false;
            feedbackCanvasGroup.interactable = false;
        }
        
        mainCamera = Camera.main;
        
        // 注册消息监听 - 使用你的消息系统
        MsgCenter.RegisterMsg(MsgConst.MSG_TICKET_CHECKED, OnTicketChecked);
    }
    
    private void OnDestroy()
    {
        // 移除消息监听
        MsgCenter.UnregisterMsg(MsgConst.MSG_TICKET_CHECKED, OnTicketChecked);
        
        // 清理动画
        currentAnimation?.Kill();
    }
    
    /// <summary>
    /// 当票被检查时调用
    /// </summary>
    private void OnTicketChecked(params object[] objs)
    {
        if (objs.Length >= 2 && objs[0] is TicketData && objs[1] is CheckResult)
        {
            TicketData ticket = (TicketData)objs[0];
            CheckResult result = (CheckResult)objs[1];
            ShowFeedback(result.outcome);
        }
    }
    
    /// <summary>
    /// 显示操作反馈
    /// </summary>
    /// <param name="outcome">票处理结果</param>
    public void ShowFeedback(TicketOutcome outcome)
    {
        // 确定是正确还是错误操作
        bool isCorrectOperation = IsCorrectOperation(outcome);
        
        // 显示反馈
        ShowFeedback(isCorrectOperation);
    }
    
    /// <summary>
    /// 显示操作反馈
    /// </summary>
    /// <param name="isCorrect">是否为正确操作</param>
    public void ShowFeedback(bool isCorrect)
    {
        // 清理之前的动画
        currentAnimation?.Kill();
        
        // 设置反馈图片
        if (feedbackImage != null)
        {
            feedbackImage.sprite = isCorrect ? correctSprite : wrongSprite;
            feedbackImage.SetNativeSize(); // 保持图片原始尺寸
        }
        
        // 定位到屏幕中央偏上位置
        if (feedbackRect != null)
        {
            feedbackRect.anchorMin = new Vector2(0.5f, 0.5f);
            feedbackRect.anchorMax = new Vector2(0.5f, 0.5f);
            feedbackRect.pivot = new Vector2(0.5f, 0.5f);
            feedbackRect.anchoredPosition = screenOffset;
        }
        
        // 重置状态
        if (feedbackCanvasGroup != null)
        {
            feedbackCanvasGroup.alpha = 0f;
            feedbackRect.localScale = Vector3.one * scaleOutAmount;
        }
        
        // 创建动画序列
        currentAnimation = DOTween.Sequence();
        
        // 淡入 + 放大
        currentAnimation.Append(feedbackCanvasGroup.DOFade(1f, fadeInDuration));
        currentAnimation.Join(feedbackRect.DOScale(Vector3.one * scaleInAmount, fadeInDuration));
        
        // 弹性效果
        currentAnimation.Append(feedbackRect.DOScale(Vector3.one, fadeInDuration * 0.5f).SetEase(Ease.OutBack));
        
        // 保持显示
        currentAnimation.AppendInterval(displayDuration);
        
        // 淡出 + 缩小
        currentAnimation.Append(feedbackCanvasGroup.DOFade(0f, fadeOutDuration));
        currentAnimation.Join(feedbackRect.DOScale(Vector3.one * scaleOutAmount, fadeOutDuration));
        
        currentAnimation.OnComplete(() => 
        {
            feedbackCanvasGroup.alpha = 0f;
        });
        
        currentAnimation.SetUpdate(true); // 即使游戏暂停也播放
    }
    
    /// <summary>
    /// 在世界坐标位置显示反馈
    /// </summary>
    public void ShowFeedbackAtWorldPosition(bool isCorrect, Vector3 worldPosition)
    {
        // 清理之前的动画
        currentAnimation?.Kill();
        
        // 设置反馈图片
        if (feedbackImage != null)
        {
            feedbackImage.sprite = isCorrect ? correctSprite : wrongSprite;
            feedbackImage.SetNativeSize();
        }
        
        // 将世界坐标转换为屏幕坐标
        if (feedbackRect != null && mainCamera != null)
        {
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
            screenPosition += (Vector3)screenOffset; // 添加偏移
            
            // 确保在屏幕范围内
            screenPosition.x = Mathf.Clamp(screenPosition.x, 100f, Screen.width - 100f);
            screenPosition.y = Mathf.Clamp(screenPosition.y, 100f, Screen.height - 100f);
            
            feedbackRect.position = screenPosition;
        }
        
        // 重置状态
        if (feedbackCanvasGroup != null)
        {
            feedbackCanvasGroup.alpha = 0f;
            feedbackRect.localScale = Vector3.one * scaleOutAmount;
        }
        
        // 创建动画序列
        currentAnimation = DOTween.Sequence();
        
        // 淡入 + 放大
        currentAnimation.Append(feedbackCanvasGroup.DOFade(1f, fadeInDuration));
        currentAnimation.Join(feedbackRect.DOScale(Vector3.one * scaleInAmount, fadeInDuration));
        
        // 弹性效果
        currentAnimation.Append(feedbackRect.DOScale(Vector3.one, fadeInDuration * 0.5f).SetEase(Ease.OutBack));
        
        // 保持显示
        currentAnimation.AppendInterval(displayDuration);
        
        // 淡出 + 缩小
        currentAnimation.Append(feedbackCanvasGroup.DOFade(0f, fadeOutDuration));
        currentAnimation.Join(feedbackRect.DOScale(Vector3.one * scaleOutAmount, fadeOutDuration));
        
        currentAnimation.OnComplete(() => 
        {
            feedbackCanvasGroup.alpha = 0f;
        });
        
        currentAnimation.SetUpdate(true);
    }
    
    /// <summary>
    /// 立即隐藏反馈
    /// </summary>
    public void HideImmediate()
    {
        currentAnimation?.Kill();
        
        if (feedbackCanvasGroup != null)
        {
            feedbackCanvasGroup.alpha = 0f;
        }
    }
    
    /// <summary>
    /// 判断操作是否正确
    /// </summary>
    private bool IsCorrectOperation(TicketOutcome outcome)
    {
        switch (outcome)
        {
            case TicketOutcome.CorrectAccept:
            case TicketOutcome.CorrectReject:
                return true;
                
            case TicketOutcome.WrongAccept:
            case TicketOutcome.WrongReject:
                return false;
                
            default:
                return false;
        }
    }
    
    /// <summary>
    /// 手动显示正确反馈（可用于测试）
    /// </summary>
    [ContextMenu("Test Correct Feedback")]
    public void TestCorrectFeedback()
    {
        ShowFeedback(true);
    }
    
    /// <summary>
    /// 手动显示错误反馈（可用于测试）
    /// </summary>
    [ContextMenu("Test Wrong Feedback")]
    public void TestWrongFeedback()
    {
        ShowFeedback(false);
    }
}