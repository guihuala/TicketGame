using System;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class TicketUI : MonoBehaviour
{
    [SerializeField] private TicketVisual visual;

    public TicketData current;
    public TicketQueueController queue;

    [Header("UV Light Effects")]
    [SerializeField] private GameObject uvLightValidEffect;    // 有效票的UV效果
    [SerializeField] private GameObject uvLightInvalidEffect;  // 无效票的UV效果
    [SerializeField] private float uvEffectDuration = 2f;      // UV效果持续时间

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
            StartCoroutine(HideUVEffectAfterDelay(uvLightValidEffect));
        }
        
        Debug.Log("[TicketUI] UV Light: 票是真实的 ✓");
    }

    private void ShowInvalidUVEffect()
    {
        if (uvLightInvalidEffect != null)
        {
            uvLightInvalidEffect.SetActive(true);
            StartCoroutine(HideUVEffectAfterDelay(uvLightInvalidEffect));
        }
        
        Debug.Log("[TicketUI] UV Light: 票是假的 ✗");
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