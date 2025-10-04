using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TicketVisual : MonoBehaviour
{
    [SerializeField] private Image ticketBg;
    [SerializeField] private Text titleText;
    [SerializeField] private Text timeText;
    [SerializeField] private Text specialText;
    [SerializeField] private Image stub;

    private Color defaultColor;
    private Vector3 defaultStubPos;

    void Start()
    {
        if (ticketBg != null)
        {
            defaultColor = ticketBg.color;  // 记录默认背景色
        }

        if (stub != null)
        {
            defaultStubPos = stub.transform.localPosition;  // 记录票根的默认位置
        }
    }

    public void SetTicket(TicketData t)
    {
        if (titleText) titleText.text = t.filmTitle;
        if (timeText) timeText.text = t.showTime;
        if (specialText) specialText.text = t.special == SpecialEventType.None ? "" : t.special.ToString();
        if (stub) stub.enabled = t.hasStub;

        if (ticketBg)
        {
            Color c = new Color(1, 1, 1, 1); // 默认背景色
            switch (t.special)
            {
                case SpecialEventType.EarlyCheck: c = new Color(1f, 1f, 0.8f, 1f); break;
                case SpecialEventType.OldTicket: c = new Color(1f, 0.9f, 0.9f, 1f); break;
                case SpecialEventType.CopyTicket: c = new Color(0.9f, 0.9f, 1f, 1f); break;
                case SpecialEventType.DrawnTicket: c = new Color(0.9f, 1f, 0.9f, 1f); break;
                case SpecialEventType.ElectronicAbuse: c = new Color(0.9f, 1f, 1f, 1f); break;
                case SpecialEventType.DamagedTicket: c = new Color(0.95f, 0.95f, 0.95f, 1f); break;
            }
            ticketBg.color = c;
        }
    }

    // 撕票成功时，添加视觉效果
    public void OnTearSuccess()
    {
        if (stub != null)
        {
            Debug.Log(stub.name);
            StartCoroutine(DropTicketStub());
        }
    }
    
    private System.Collections.IEnumerator DropTicketStub()
    {
        // 目标位置（票根掉落的目标位置）
        Vector3 targetPos = new Vector3(defaultStubPos.x, defaultStubPos.y - 100f, defaultStubPos.z);
        
        float duration = 1f;  // 动画持续时间
        stub.transform.DOLocalMove(targetPos, duration).SetEase(Ease.InOutQuad);

        yield return new WaitForSeconds(duration);
    }
}
