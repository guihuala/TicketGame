using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HintUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup hintPanel;
    [SerializeField] private Text hintText;
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float defaultDisplayDuration = 2f;

    private void Awake()
    {
        MsgCenter.RegisterMsg(MsgConst.MSG_SHOW_HINT, OnShowHint);
    }

    private void Start()
    {
        // 初始隐藏
        hintPanel.alpha = 0f;
    }

    private void OnDestroy()
    {
        MsgCenter.UnregisterMsg(MsgConst.MSG_SHOW_HINT, OnShowHint);
    }
    
    private void OnShowHint(params object[] args)
    {
        if (args.Length >= 1 && args[0] is string hintText)
        {
            float duration = defaultDisplayDuration;
            if (args.Length >= 2 && args[1] is float customDuration)
            {
                duration = customDuration;
            }
            ShowHint(hintText, duration);
        }
    }

    public void ShowHint(string text, float duration = 2f)
    {
        hintText.text = text;
        hintPanel.gameObject.SetActive(true);

        // 停止之前的动画
        hintPanel.DOKill();
        
        // 淡入动画
        hintPanel.DOFade(1f, fadeDuration);

        // 定时淡出
        hintPanel.DOFade(0f, fadeDuration)
            .SetDelay(fadeDuration + duration)
            .OnComplete(() => {
                hintPanel.gameObject.SetActive(false);
            });
    }
}