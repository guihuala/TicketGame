using DG.Tweening;
using UnityEngine;

/// <summary>
/// UI面板的基类
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class BasePanel : MonoBehaviour
{
    protected bool hasRemoved = false;
    protected string panelName;
    protected CanvasGroup canvasGroup;
    
    [Header("基础动画设置")]
    [SerializeField] protected PanelAnimationType animationType = PanelAnimationType.Fade; // 动画类型
    [SerializeField] protected float fadeInDuration = 0.5f;
    [SerializeField] protected float fadeOutDuration = 0.5f;
    [SerializeField] protected Ease fadeInEase = Ease.OutQuad;
    [SerializeField] protected Ease fadeOutEase = Ease.InQuad;
    
    [Header("缩放动画设置")]
    [SerializeField] protected bool scaleAnimation = true;
    [SerializeField] protected Vector2 scaleFrom = new Vector2(0.8f, 0.8f);
    [SerializeField] protected float scaleDuration = 0.3f;
    
    [Header("滑动动画设置")]
    [SerializeField] protected SlideDirection slideDirection = SlideDirection.Left; // 滑动方向
    [SerializeField] protected float slideDistance = 100f; // 滑动距离
    [SerializeField] protected float slideDuration = 0.5f; // 滑动持续时间
    
    [Header("弹性动画设置")]
    [SerializeField] protected float elasticStrength = 0.5f; // 弹性强度
    [SerializeField] protected int elasticVibrato = 10; // 振动次数
    [SerializeField] protected float elasticDuration = 0.6f; // 弹性动画持续时间
    
    [Header("组合动画设置")]
    [SerializeField] protected bool useCombinedAnimations = false; // 是否使用组合动画
    [SerializeField] protected PanelAnimationType[] combinedAnimationTypes; // 组合动画类型
    
    // 记录原始位置，用于动画
    protected Vector3 originalPosition;
    
    /// <summary>
    /// 面板动画类型
    /// </summary>
    public enum PanelAnimationType
    {
        Fade,           // 淡入淡出
        Slide,          // 滑动
        Scale,          // 缩放
        Elastic,        // 弹性
        Bounce,         // 弹跳
        Flip            // 翻转
    }
    
    /// <summary>
    /// 滑动方向
    /// </summary>
    public enum SlideDirection
    {
        Left,
        Right,
        Top,
        Bottom
    }

    protected virtual void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        originalPosition = transform.localPosition; // 记录原始位置
    }

    /// <summary>
    /// 打开面板
    /// </summary>
    public virtual void OpenPanel(string name)
    {
        panelName = name;
        gameObject.SetActive(true);
        
        // 初始化面板状态
        InitializePanelState();
        
        // 播放入场动画
        PlayEnterAnimation();
    }

    /// <summary>
    /// 初始化面板状态
    /// </summary>
    protected virtual void InitializePanelState()
    {
        canvasGroup.alpha = 0;
        
        // 根据动画类型设置初始状态
        switch (animationType)
        {
            case PanelAnimationType.Scale:
                transform.localScale = scaleFrom;
                break;
                
            case PanelAnimationType.Slide:
                SetSlideStartPosition();
                break;
                
            case PanelAnimationType.Elastic:
            case PanelAnimationType.Bounce:
                transform.localScale = Vector3.zero;
                break;
                
            case PanelAnimationType.Flip:
                transform.localRotation = Quaternion.Euler(0, 90, 0);
                break;
        }
        
        // 重置位置（对于滑动动画以外的类型）
        if (animationType != PanelAnimationType.Slide)
        {
            transform.localPosition = originalPosition;
        }
    }

    /// <summary>
    /// 设置滑动起始位置
    /// </summary>
    protected virtual void SetSlideStartPosition()
    {
        Vector3 startPosition = originalPosition;
        
        switch (slideDirection)
        {
            case SlideDirection.Left:
                startPosition.x -= slideDistance;
                break;
            case SlideDirection.Right:
                startPosition.x += slideDistance;
                break;
            case SlideDirection.Top:
                startPosition.y += slideDistance;
                break;
            case SlideDirection.Bottom:
                startPosition.y -= slideDistance;
                break;
        }
        
        transform.localPosition = startPosition;
    }

    /// <summary>
    /// 播放入场动画
    /// </summary>
    protected virtual void PlayEnterAnimation()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.SetUpdate(UpdateType.Normal, true);
        
        if (useCombinedAnimations && combinedAnimationTypes.Length > 0)
        {
            // 使用组合动画
            PlayCombinedEnterAnimation(sequence);
        }
        else
        {
            // 使用单一动画
            PlaySingleEnterAnimation(sequence);
        }
        
        // 淡入动画（基础）
        sequence.Join(canvasGroup.DOFade(1, fadeInDuration)
            .SetEase(fadeInEase));
    }

    /// <summary>
    /// 播放单一入场动画
    /// </summary>
    protected virtual void PlaySingleEnterAnimation(Sequence sequence)
    {
        switch (animationType)
        {
            case PanelAnimationType.Fade:
                // 基础淡入已在主序列中处理
                break;
                
            case PanelAnimationType.Scale:
                sequence.Join(transform.DOScale(Vector3.one, scaleDuration)
                    .SetEase(fadeInEase));
                break;
                
            case PanelAnimationType.Slide:
                sequence.Join(transform.DOLocalMove(originalPosition, slideDuration)
                    .SetEase(fadeInEase));
                break;
                
            case PanelAnimationType.Elastic:
                sequence.Join(transform.DOScale(Vector3.one, elasticDuration)
                    .SetEase(Ease.OutElastic, elasticStrength, elasticVibrato));
                break;
                
            case PanelAnimationType.Bounce:
                sequence.Join(transform.DOScale(Vector3.one, elasticDuration)
                    .SetEase(Ease.OutBounce));
                break;
                
            case PanelAnimationType.Flip:
                sequence.Join(transform.DOLocalRotate(Vector3.zero, fadeInDuration)
                    .SetEase(fadeInEase));
                break;
        }
    }

    /// <summary>
    /// 播放组合入场动画
    /// </summary>
    protected virtual void PlayCombinedEnterAnimation(Sequence sequence)
    {
        foreach (var animType in combinedAnimationTypes)
        {
            switch (animType)
            {
                case PanelAnimationType.Scale:
                    if (animationType != PanelAnimationType.Scale) // 避免重复
                    {
                        transform.localScale = scaleFrom;
                        sequence.Join(transform.DOScale(Vector3.one, scaleDuration)
                            .SetEase(fadeInEase));
                    }
                    break;
                    
                case PanelAnimationType.Slide:
                    if (animationType != PanelAnimationType.Slide) // 避免重复
                    {
                        SetSlideStartPosition();
                        sequence.Join(transform.DOLocalMove(originalPosition, slideDuration)
                            .SetEase(fadeInEase));
                    }
                    break;
                    
                case PanelAnimationType.Elastic:
                    sequence.Join(transform.DOPunchScale(Vector3.one * elasticStrength, elasticDuration, elasticVibrato)
                        .SetEase(Ease.OutElastic));
                    break;
            }
        }
    }

    /// <summary>
    /// 关闭面板
    /// </summary>
    public virtual void ClosePanel()
    {
        hasRemoved = true;

        Sequence sequence = DOTween.Sequence();
        sequence.SetUpdate(UpdateType.Normal, true);
        
        // 淡出动画
        sequence.Append(canvasGroup.DOFade(0, fadeOutDuration)
            .SetEase(fadeOutEase));
        
        // 根据动画类型播放退出动画
        switch (animationType)
        {
            case PanelAnimationType.Scale:
                sequence.Join(transform.DOScale(scaleFrom, Mathf.Min(fadeOutDuration, scaleDuration))
                    .SetEase(fadeOutEase));
                break;
                
            case PanelAnimationType.Slide:
                sequence.Join(transform.DOLocalMove(GetSlideEndPosition(), Mathf.Min(fadeOutDuration, slideDuration))
                    .SetEase(fadeOutEase));
                break;
                
            case PanelAnimationType.Elastic:
            case PanelAnimationType.Bounce:
                sequence.Join(transform.DOScale(Vector3.zero, fadeOutDuration)
                    .SetEase(fadeOutEase));
                break;
                
            case PanelAnimationType.Flip:
                sequence.Join(transform.DOLocalRotate(new Vector3(0, 90, 0), fadeOutDuration)
                    .SetEase(fadeOutEase));
                break;
        }
        
        sequence.OnComplete(() =>
        {
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        });
    }

    /// <summary>
    /// 获取滑动结束位置
    /// </summary>
    protected virtual Vector3 GetSlideEndPosition()
    {
        Vector3 endPosition = originalPosition;
        
        switch (slideDirection)
        {
            case SlideDirection.Left:
                endPosition.x -= slideDistance;
                break;
            case SlideDirection.Right:
                endPosition.x += slideDistance;
                break;
            case SlideDirection.Top:
                endPosition.y += slideDistance;
                break;
            case SlideDirection.Bottom:
                endPosition.y -= slideDistance;
                break;
        }
        
        return endPosition;
    }

    /// <summary>
    /// 设置动画类型
    /// </summary>
    public virtual void SetAnimationType(PanelAnimationType type)
    {
        animationType = type;
    }

    /// <summary>
    /// 设置滑动方向
    /// </summary>
    public virtual void SetSlideDirection(SlideDirection direction)
    {
        slideDirection = direction;
    }

    /// <summary>
    /// 设置组合动画
    /// </summary>
    public virtual void SetCombinedAnimations(params PanelAnimationType[] animationTypes)
    {
        useCombinedAnimations = true;
        combinedAnimationTypes = animationTypes;
    }

    // 以下方法保持不变
    public void CloseThisPanel()
    {
        UIManager.Instance.ClosePanel(panelName);
    }
    
    public virtual void ClosePanelImmediate()
    {
        hasRemoved = true;
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }
    
    public virtual void SetInteractable(bool interactable)
    {
        if (canvasGroup != null)
        {
            canvasGroup.interactable = interactable;
            canvasGroup.blocksRaycasts = interactable;
        }
    }
}