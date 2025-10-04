using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class TicketVisual : MonoBehaviour
{
    [SerializeField] private Image ticketBg;
    [SerializeField] private Text titleText;
    [SerializeField] private Text timeText;
    [SerializeField] private Text specialText;
    [SerializeField] private Image stub;
    [SerializeField] private Image warningIcon; // 警告图标
    [SerializeField] private Image damageOverlay; // 损坏覆盖层
    [SerializeField] private Image drawnEffect; // 手绘效果

    private Color defaultColor;
    private Vector3 defaultStubPos;

    void Start()
    {
        if (ticketBg != null)
        {
            defaultColor = ticketBg.color;
        }

        if (stub != null)
        {
            defaultStubPos = stub.transform.localPosition;
        }
    }

    public void SetTicket(TicketData t)
    {
        // 重置所有视觉效果
        ResetVisuals();

        // 设置基本信息
        if (titleText) titleText.text = t.filmTitle;
        if (timeText) timeText.text = t.showTime;
        
        // 设置特殊事件文本
        string specialDisplayText = GetSpecialDisplayText(t.special);
        if (specialText) 
        {
            specialText.text = specialDisplayText;
            specialText.color = GetSpecialTextColor(t.special);
        }

        // 设置票根显示
        if (stub) stub.enabled = t.hasStub;

        // 根据票的类型应用不同的视觉效果
        ApplyVisualEffects(t);
    }

    private void ResetVisuals()
    {
        // 重置背景色
        if (ticketBg) ticketBg.color = defaultColor;
        
        // 隐藏所有效果
        if (warningIcon) warningIcon.gameObject.SetActive(false);
        if (damageOverlay) damageOverlay.gameObject.SetActive(false);
        if (drawnEffect) drawnEffect.gameObject.SetActive(false);
        
        // 重置文本颜色
        if (titleText) titleText.color = Color.black;
        if (timeText) timeText.color = Color.black;
        if (specialText) specialText.color = Color.gray;
    }

    private void ApplyVisualEffects(TicketData t)
    {
        switch (t.special)
        {
            case SpecialEventType.None:
                // 正常票：绿色边框
                if (ticketBg) ticketBg.color = new Color(0.9f, 1f, 0.9f, 1f);
                break;

            case SpecialEventType.EarlyCheck:
                // 提前检票：黄色背景 + 时钟图标
                if (ticketBg) ticketBg.color = new Color(1f, 1f, 0.8f, 1f);
                if (warningIcon) 
                {
                    warningIcon.gameObject.SetActive(true);
                    warningIcon.color = Color.yellow;
                }
                break;

            case SpecialEventType.OldTicket:
                // 旧票：褪色效果 + 过期图标
                if (ticketBg) ticketBg.color = new Color(0.8f, 0.8f, 0.8f, 0.8f);
                if (titleText) titleText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                if (timeText) timeText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                if (warningIcon) 
                {
                    warningIcon.gameObject.SetActive(true);
                    warningIcon.color = Color.red;
                }
                break;

            case SpecialEventType.WrongNameSpelling:
                // 错误命名：红色高亮电影名 + 闪烁效果
                if (titleText) 
                {
                    titleText.color = Color.red;
                    titleText.fontStyle = FontStyle.Bold;
                    // 添加闪烁动画
                    StartCoroutine(BlinkText(titleText));
                }
                if (warningIcon) 
                {
                    warningIcon.gameObject.SetActive(true);
                    warningIcon.color = Color.red;
                }
                break;

            case SpecialEventType.DamagedTicket:
                // 受损票：污渍覆盖层
                if (ticketBg) ticketBg.color = new Color(0.95f, 0.95f, 0.95f, 1f);
                if (damageOverlay) 
                {
                    damageOverlay.gameObject.SetActive(true);
                    damageOverlay.color = new Color(0.3f, 0.3f, 0.3f, 0.3f);
                }
                break;

            case SpecialEventType.MissingStub:
                // 缺失票根：红色叉号在票根位置
                if (stub) 
                {
                    stub.enabled = true;
                    stub.color = Color.red;
                }
                if (warningIcon) 
                {
                    warningIcon.gameObject.SetActive(true);
                    warningIcon.color = Color.red;
                }
                break;

            case SpecialEventType.DrawnTicket:
                // 手画票：手绘纹理 + 不规则边框
                if (ticketBg) ticketBg.color = new Color(0.9f, 1f, 0.9f, 1f);
                if (drawnEffect) 
                {
                    drawnEffect.gameObject.SetActive(true);
                    drawnEffect.color = new Color(1f, 1f, 1f, 0.3f);
                }
                if (titleText) titleText.fontStyle = FontStyle.Italic;
                break;

            case SpecialEventType.CopyTicket:
                // 复制票：灰色背景 + 复印效果
                if (ticketBg) ticketBg.color = new Color(0.9f, 0.9f, 1f, 0.7f);
                if (titleText) titleText.color = new Color(0.3f, 0.3f, 0.3f, 1f);
                if (timeText) timeText.color = new Color(0.3f, 0.3f, 0.3f, 1f);
                break;

            case SpecialEventType.ElectronicAbuse:
                // 电子票滥用：蓝色边框 + 屏幕像素效果
                if (ticketBg) ticketBg.color = new Color(0.9f, 1f, 1f, 1f);
                // 可以添加像素化shader效果
                break;
        }

        // 额外效果：如果没有票根，添加特殊提示
        if (!t.hasStub && stub)
        {
            stub.enabled = true;
            stub.color = Color.red;
            // 添加"No Stub"文字
            if (specialText) specialText.text = "NO STUB - " + specialText.text;
        }
    }

    private string GetSpecialDisplayText(SpecialEventType special)
    {
        switch (special)
        {
            case SpecialEventType.None: return "";
            case SpecialEventType.EarlyCheck: return "Early Check-in";
            case SpecialEventType.OldTicket: return "Expired Ticket";
            case SpecialEventType.WrongNameSpelling: return "Wrong Film Name";
            case SpecialEventType.DamagedTicket: return "Damaged Ticket";
            case SpecialEventType.MissingStub: return "Missing Stub";
            case SpecialEventType.DrawnTicket: return "Hand-drawn Ticket";
            case SpecialEventType.CopyTicket: return "Photocopy Ticket";
            case SpecialEventType.ElectronicAbuse: return "Electronic Abuse";
            default: return special.ToString();
        }
    }

    private Color GetSpecialTextColor(SpecialEventType special)
    {
        switch (special)
        {
            case SpecialEventType.EarlyCheck: return Color.yellow;
            case SpecialEventType.OldTicket: return Color.red;
            case SpecialEventType.WrongNameSpelling: return Color.red;
            case SpecialEventType.MissingStub: return Color.red;
            case SpecialEventType.DamagedTicket: return Color.gray;
            case SpecialEventType.DrawnTicket: return Color.green;
            case SpecialEventType.CopyTicket: return Color.blue;
            case SpecialEventType.ElectronicAbuse: return Color.cyan;
            default: return Color.gray;
        }
    }

    private IEnumerator BlinkText(Text text)
    {
        Color originalColor = text.color;
        while (text != null)
        {
            text.color = text.color == originalColor ? new Color(1f, 0.5f, 0.5f, 1f) : originalColor;
            yield return new WaitForSeconds(0.5f);
        }
    }

    // 撕票成功时，添加视觉效果
    public void OnTearSuccess()
    {
        if (stub != null)
        {
            StartCoroutine(DropTicketStub());
        }
        
        // 添加整体票面动画
        if (ticketBg != null)
        {
            ticketBg.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0f), 0.3f).SetUpdate(true);
        }
    }

    private IEnumerator DropTicketStub()
    {
        Vector3 targetPos = new Vector3(defaultStubPos.x, defaultStubPos.y - 100f, defaultStubPos.z);
        float duration = 1f;
        
        stub.transform.DOLocalMove(targetPos, duration)
            .SetEase(Ease.InOutQuad)
            .SetUpdate(true);

        // 同时添加淡出效果
        stub.DOFade(0f, duration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true);

        yield return new WaitForSeconds(duration);
    }
}