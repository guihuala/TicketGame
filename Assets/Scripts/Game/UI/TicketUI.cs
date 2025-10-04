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
        Debug.Log($"Ticket type is {data.special},Ticket is{data.isValid}");
        if (visual != null) visual.SetTicket(current);
    }

    private void OnAcceptTicket()
    {
        if (queue != null && queue.IsWaitingForInput())
        {
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
}