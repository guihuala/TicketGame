using UnityEngine;

public class TicketUI : MonoBehaviour
{
    [SerializeField] private TicketVisual visual;

    private TicketData current;
    public TicketQueueController queue;  // 自动绑定

    public void BindTicket(TicketData data)
    {
        current = data;
        if (visual != null) visual.SetTicket(current);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))  // 左键接受
        {
            OnAcceptTicket();
        }
        else if (Input.GetMouseButtonDown(1))  // 右键拒绝
        {
            OnRejectTicket();
        }
    }

    private void OnAcceptTicket()
    {
        Debug.Log("Ticket Accepted");
        visual.OnTearSuccess();  // 保留撕票动画
        queue.AcceptCurrentTicket();  // 接受票
    }

    private void OnRejectTicket()
    {
        Debug.Log("Ticket Rejected");
        queue.RejectCurrentTicket();  // 拒绝票
    }
}