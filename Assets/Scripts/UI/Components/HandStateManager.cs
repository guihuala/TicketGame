using UnityEngine;
using UnityEngine.UI;

public class HandStateManager : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator handAnimator;
    
    [Header("动画参数名称")]
    [SerializeField] private string hasTicketParam = "HasTicket";
    [SerializeField] private string resetTrigger = "Reset";
    [SerializeField] private string useUVLightTrigger = "UseUVLight"; // 新增的UV灯动画触发器

    void Start()
    {
        MsgCenter.RegisterMsg(MsgConst.MSG_TICKET_SPAWNED, OnTicketSpawned);
        MsgCenter.RegisterMsg(MsgConst.MSG_TICKET_CHECKED, OnTicketChecked);
        MsgCenter.RegisterMsg(MsgConst.MSG_USE_UV_LIGHT, OnUseUVLight); // 注册UV灯使用消息
        
        // 初始设置为没票状态
        SetUITicketState(false);
    }

    void OnDestroy()
    {
        // 取消消息监听
        MsgCenter.UnregisterMsg(MsgConst.MSG_TICKET_SPAWNED, OnTicketSpawned);
        MsgCenter.UnregisterMsg(MsgConst.MSG_TICKET_CHECKED, OnTicketChecked);
        MsgCenter.UnregisterMsg(MsgConst.MSG_USE_UV_LIGHT, OnUseUVLight); // 取消UV灯消息监听
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

    private void OnUseUVLight(params object[] objs)
    {
        // 播放UV灯使用动画
        if (handAnimator != null)
        {
            handAnimator.SetTrigger(useUVLightTrigger);
            Debug.Log("[HandStateManager] 播放UV灯动画");
        }
    }

    private void SetUITicketState(bool hasTicket)
    {
        // 设置动画参数
        handAnimator.SetBool(hasTicketParam, hasTicket);
    }
}