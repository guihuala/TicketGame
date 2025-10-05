using UnityEngine;
using UnityEngine.UI;

public class HandStateManager : MonoBehaviour
{
    [Header("UI图片组")]
    [SerializeField] private Image[] uiImages;
    
    [Header("有票时的图片")]
    [SerializeField] private Sprite[] ticketActiveSprites;
    
    [Header("没票时的图片")]
    [SerializeField] private Sprite[] ticketInactiveSprites;

    void Start()
    {
        MsgCenter.RegisterMsg(MsgConst.MSG_TICKET_SPAWNED, OnTicketSpawned);
        MsgCenter.RegisterMsg(MsgConst.MSG_TICKET_CHECKED, OnTicketChecked);
        
        // 初始设置为没票状态
        SetUITicketState(false);
    }

    void OnDestroy()
    {
        // 取消消息监听
        MsgCenter.UnregisterMsg(MsgConst.MSG_TICKET_SPAWNED, OnTicketSpawned);
        MsgCenter.UnregisterMsg(MsgConst.MSG_TICKET_CHECKED, OnTicketChecked);
    }

    private void OnTicketSpawned(params object[] objs)
    {
        // 有新票生成，切换到有票状态
        if (objs.Length > 0 && objs[0] is TicketData)
        {
            SetUITicketState(true);
        }
    }

    private void OnTicketChecked(params object[] objs)
    {
        // 票被处理，切换到没票状态
        if (objs.Length > 1 && objs[0] is TicketData && objs[1] is CheckResult)
        {
            SetUITicketState(false);
        }
    }

    private void SetUITicketState(bool hasTicket)
    {
        if (uiImages == null || uiImages.Length == 0) return;
        
        Sprite[] targetSprites = hasTicket ? ticketActiveSprites : ticketInactiveSprites;
        
        // 应用图片切换
        for (int i = 0; i < uiImages.Length && i < targetSprites.Length; i++)
        {
            if (uiImages[i] != null && targetSprites[i] != null)
            {
                uiImages[i].sprite = targetSprites[i];
            }
        }
    }
}