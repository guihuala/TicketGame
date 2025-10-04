using UnityEngine;

public class TicketUI : MonoBehaviour
{
    [SerializeField] private TicketVisual visual;

    private TicketData current;
    public TicketQueueController queue;  // 自动绑定

    private void OnEnable()
    {
        // 注册输入事件
        InputController.Instance.onAcceptTicket += OnAcceptTicket;
        InputController.Instance.onRejectTicket += OnRejectTicket;
    }

    private void OnDisable()
    {
        // 取消注册输入事件
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
        Debug.Log("Ticket Accepted");
        visual.OnTearSuccess();
        queue.AcceptCurrentTicket();  // 接受票
    }

    private void OnRejectTicket()
    {
        Debug.Log("Ticket Rejected");
        queue.RejectCurrentTicket();  // 拒绝票
    }
}