using UnityEngine;

public class TicketUI : MonoBehaviour
{
    [SerializeField] private TicketVisual visual;

    private TicketData current;
    public TicketQueueController queue;

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
        // 只有在等待玩家输入时才响应
        if (queue != null && queue.IsWaitingForInput())
        {
            Debug.Log("Ticket Accepted");
            queue.AcceptCurrentTicket();
        }
    }

    private void OnRejectTicket()
    {
        // 只有在等待玩家输入时才响应
        if (queue != null && queue.IsWaitingForInput())
        {
            Debug.Log("Ticket Rejected");
            queue.RejectCurrentTicket();
        }
    }
}