using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class VIPPassUI : MonoBehaviour
{
    [Header("VIP票UI")]
    [SerializeField] private Image vipPassImage;
    [SerializeField] private float animationDuration = 1.5f;
    [SerializeField] private float displayDuration = 1f;

    private CanvasGroup canvasGroup;
    private bool isShowing = false;
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Sequence currentAnimationSequence;

    void Start()
    {
        // 确保有CanvasGroup组件
        canvasGroup = vipPassImage.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = vipPassImage.gameObject.AddComponent<CanvasGroup>();
        }

        // 保存原始状态
        originalScale = vipPassImage.transform.localScale;
        originalPosition = vipPassImage.transform.localPosition;

        // 初始隐藏
        HideVIPPassUI();
        
        // 注册消息监听
        MsgCenter.RegisterMsg(MsgConst.MSG_USE_VIP_PASS, OnUseVIPPass);
    }

    void OnDestroy()
    {
        // 取消消息监听
        MsgCenter.UnregisterMsg(MsgConst.MSG_USE_VIP_PASS, OnUseVIPPass);
        
        // 清理DOTween动画
        currentAnimationSequence?.Kill();
    }

    private void OnUseVIPPass(params object[] objs)
    {
        ShowVIPPassAnimation();
    }

    public void ShowVIPPassAnimation()
    {
        if (isShowing) return;
        
        isShowing = true;
        PlayVIPPassTweenAnimation();
    }

    private void PlayVIPPassTweenAnimation()
    {
        // 重置状态
        vipPassImage.gameObject.SetActive(true);
        vipPassImage.transform.localScale = Vector3.zero;
        vipPassImage.transform.localRotation = Quaternion.identity;
        canvasGroup.alpha = 0f;

        // 清理之前的动画
        currentAnimationSequence?.Kill();

        // 创建新的动画序列
        currentAnimationSequence = DOTween.Sequence();

        // 第一阶段：旋转入场
        currentAnimationSequence.Append(vipPassImage.transform.DOScale(originalScale * 1.2f, animationDuration * 0.4f)
            .SetEase(Ease.OutBack));
        currentAnimationSequence.Join(vipPassImage.transform.DORotate(new Vector3(0, 0, 360), animationDuration * 0.4f, RotateMode.FastBeyond360)
            .SetEase(Ease.OutCubic));
        currentAnimationSequence.Join(canvasGroup.DOFade(1f, animationDuration * 0.3f));

        // 第二阶段：轻微回弹
        currentAnimationSequence.Append(vipPassImage.transform.DOScale(originalScale, animationDuration * 0.2f)
            .SetEase(Ease.OutElastic));

        // 第三阶段：停留显示
        currentAnimationSequence.AppendInterval(displayDuration);

        // 第四阶段：旋转退场
        currentAnimationSequence.Append(vipPassImage.transform.DOScale(Vector3.zero, animationDuration * 0.4f)
            .SetEase(Ease.InBack));
        currentAnimationSequence.Join(vipPassImage.transform.DORotate(new Vector3(0, 0, -180), animationDuration * 0.4f, RotateMode.LocalAxisAdd)
            .SetEase(Ease.InCubic));
        currentAnimationSequence.Join(canvasGroup.DOFade(0f, animationDuration * 0.3f));

        // 动画完成回调
        currentAnimationSequence.OnComplete(() =>
        {
            HideVIPPassUI();
            isShowing = false;
            currentAnimationSequence = null;
        });

        currentAnimationSequence.Play();
    }

    private void HideVIPPassUI()
    {
        if (vipPassImage != null)
        {
            vipPassImage.gameObject.SetActive(false);
            // 恢复原始状态
            vipPassImage.transform.localScale = originalScale;
            vipPassImage.transform.localPosition = originalPosition;
            vipPassImage.transform.localRotation = Quaternion.identity;
        }
    }

    // 强制停止动画（如果需要）
    public void StopAnimation()
    {
        currentAnimationSequence?.Kill();
        HideVIPPassUI();
        isShowing = false;
    }
}