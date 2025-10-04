using UnityEngine;

public class TicketUI : MonoBehaviour
{
    [SerializeField] private TicketQueueController queue;
    [SerializeField] private TearInteraction tear;
    [SerializeField] private TicketVisual visual;

    private TicketData current;

    public void BindTicket(TicketData data)
    {
        current = data;
        if (visual != null) visual.SetTicket(current);
        if (tear != null) tear.Prepare();
    }

    public void OnRejectClicked() => queue.RejectCurrentTicket();
    public void OnTearSucceeded() => queue.AcceptCurrentTicket();
}
