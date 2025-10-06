using System;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;
using DG.Tweening;

public class TicketUI : MonoBehaviour
{
    [SerializeField] private TicketVisual visual;

    public TicketData current;
    public TicketQueueController queue;

    [Header("UV Light Effects")]
    [SerializeField] private GameObject uvLightValidEffect;    // 有效票的UV效果
    [SerializeField] private GameObject uvLightInvalidEffect;  // 无效票的UV效果
    [SerializeField] private float uvEffectDuration = 2f;      // UV效果持续时间
    [SerializeField] private float rotationDuration = 0.5f;    // 旋转动画持续时间
    [SerializeField] private float rotationAngle = 180f;       // 旋转角度

    private void Start()
    {
        uvLightValidEffect.SetActive(false);
        uvLightInvalidEffect.SetActive(false);
    }

    public void ShowUVLightEffect(bool isValid)
    {
        if (isValid)
        {
            ShowValidUVEffect();
        }
        else
        {
            ShowInvalidUVEffect();
        }
    }

    private void ShowValidUVEffect()
    {
        if (uvLightValidEffect != null)
        {
            uvLightValidEffect.SetActive(true);
            PlayRotationAnimation(uvLightValidEffect.transform);
            StartCoroutine(HideUVEffectAfterDelay(uvLightValidEffect));
        }
        
        Debug.Log("[TicketUI] UV Light: 票是真实的 ✓");
    }

    private void ShowInvalidUVEffect()
    {
        if (uvLightInvalidEffect != null)
        {
            uvLightInvalidEffect.SetActive(true);
            PlayRotationAnimation(uvLightInvalidEffect.transform);
            StartCoroutine(HideUVEffectAfterDelay(uvLightInvalidEffect));
        }
        
        Debug.Log("[TicketUI] UV Light: 票是假的 ✗");
    }

    /// <summary>
    /// 播放旋转动画
    /// </summary>
    private void PlayRotationAnimation(Transform effectTransform)
    {
        // 重置旋转
        effectTransform.localRotation = Quaternion.identity;
        
        // 使用DOTween创建旋转动画
        effectTransform.DORotate(new Vector3(0, 0, rotationAngle), rotationDuration, RotateMode.LocalAxisAdd)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                // 动画完成后可以添加其他效果
                Debug.Log("[TicketUI] 旋转动画完成");
            });
    }

    private IEnumerator HideUVEffectAfterDelay(GameObject effect)
    {
        yield return new WaitForSeconds(uvEffectDuration);
        if (effect != null)
        {
            effect.SetActive(false);
        }
    }

    private string[] tearSounds = { "Tear_1", "Tear_2", "Tear_3" };
    
    private void OnEnable()
    {
        InputController.Instance.onAcceptTicket += OnAcceptTicket;
        InputController.Instance.onRejectTicket += OnRejectTicket;
    }

    private void OnDisable()
    {
        if (InputController.Instance != null)
        {
            InputController.Instance.onAcceptTicket -= OnAcceptTicket;
            InputController.Instance.onRejectTicket -= OnRejectTicket;
        }
    }

    public void BindTicket(TicketData data)
    {
        current = data;
        if (visual != null) visual.SetTicket(current);
    }

    private void OnAcceptTicket()
    {
        if (queue != null && queue.IsWaitingForInput())
        {
            PlayRandomTearSound();
            queue.AcceptCurrentTicket();
        }
    }

    private void OnRejectTicket()
    {
        if (queue != null && queue.IsWaitingForInput())
        {
            queue.RejectCurrentTicket();
        }
    }
    
    private void PlayRandomTearSound()
    {
        int randomIndex = Random.Range(0, tearSounds.Length);
        AudioManager.Instance.PlaySfx(tearSounds[randomIndex]);
    }
}