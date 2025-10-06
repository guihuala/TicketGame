using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BroadcastSystemUI : MonoBehaviour
{
    [Header("广播系统UI")]
    [SerializeField] private CanvasGroup broadcastPanel;
    [SerializeField] private Image broadcastBackground;
    [SerializeField] private Text filmTitleText;

    
    [Header("动画设置")]
    [SerializeField] private float slideInDuration = 0.6f;
    [SerializeField] private float displayDuration = 3.5f;
    [SerializeField] private float slideOutDuration = 0.6f;
    [SerializeField] private float shakeIntensity = 10f;
    
    private Vector2 originalPosition;
    private Vector2 hiddenPosition;
    private bool isShowing = false;
    private Sequence currentAnimationSequence;

    void Start()
    {
        originalPosition = broadcastPanel.transform.localPosition;
        hiddenPosition = originalPosition + new Vector2(400f, 0f); // 从右侧滑入
        
        // 初始隐藏
        broadcastPanel.alpha = 0f;
        broadcastPanel.transform.localPosition = hiddenPosition;
        broadcastPanel.gameObject.SetActive(false);
        
        MsgCenter.RegisterMsg(MsgConst.MSG_BROADCAST_DELAY, OnBroadcastDelay);
    }

    void OnDestroy()
    {
        MsgCenter.UnregisterMsg(MsgConst.MSG_BROADCAST_DELAY, OnBroadcastDelay);
        currentAnimationSequence?.Kill();
    }

    private void OnBroadcastDelay(params object[] objs)
    {
        int delayMinutes = 15;
        string filmTitle = "Next Film";
        
        if (objs.Length > 0 && objs[0] is int minutes)
        {
            delayMinutes = minutes;
        }
        if (objs.Length > 1 && objs[1] is string title)
        {
            filmTitle = title;
        }
        
        ShowBroadcastAnimation(delayMinutes, filmTitle);
    }

    public void ShowBroadcastAnimation(int delayMinutes, string filmTitle)
    {
        if (isShowing) return;
        
        isShowing = true;
        PlayEnhancedBroadcastAnimation(delayMinutes, filmTitle);
    }

    private void PlayEnhancedBroadcastAnimation(int delayMinutes, string filmTitle)
    {

        filmTitleText.text = filmTitle;
        
        currentAnimationSequence?.Kill();

        // 重置状态
        broadcastPanel.gameObject.SetActive(true);
        broadcastPanel.alpha = 0f;
        broadcastPanel.transform.localPosition = hiddenPosition;
        broadcastBackground.transform.localScale = Vector3.one * 0.8f;

        currentAnimationSequence = DOTween.Sequence();

        // 1. 快速滑入 + 淡入 + 背景缩放
        currentAnimationSequence.Append(broadcastPanel.transform.DOLocalMove(originalPosition, slideInDuration)
            .SetEase(Ease.OutBack));
        currentAnimationSequence.Join(broadcastPanel.DOFade(1f, slideInDuration * 0.8f));
        currentAnimationSequence.Join(broadcastBackground.transform.DOScale(Vector3.one, slideInDuration)
            .SetEase(Ease.OutElastic));

        // 3. 轻微震动效果
        currentAnimationSequence.Append(broadcastPanel.transform.DOShakePosition(0.5f, shakeIntensity, 10, 90f));

        // 4. 停留显示
        currentAnimationSequence.AppendInterval(displayDuration);

        // 5. 滑出 + 淡出
        currentAnimationSequence.Append(broadcastPanel.transform.DOLocalMove(hiddenPosition, slideOutDuration)
            .SetEase(Ease.InBack));
        currentAnimationSequence.Join(broadcastPanel.DOFade(0f, slideOutDuration));

        currentAnimationSequence.OnComplete(() =>
        {
            broadcastPanel.gameObject.SetActive(false);
            isShowing = false;
            currentAnimationSequence = null;
        });

        currentAnimationSequence.Play();
    }

    public void StopAnimation()
    {
        currentAnimationSequence?.Kill();
        broadcastPanel.gameObject.SetActive(false);
        isShowing = false;
    }
}