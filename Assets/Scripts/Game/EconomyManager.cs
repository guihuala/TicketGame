using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public int currentIncome { get; private set; }
    private DaySchedule currentLevel;

    public void ResetIncome() => currentIncome = 0;

    public void SetCurrentLevel(DaySchedule level)
    {
        currentLevel = level;
        ResetIncome();
    }

    public void ApplyResult(CheckResult r)
    {
        int newIncome = currentIncome + r.incomeDelta;
        
        // 禁止金钱低于零
        currentIncome = Mathf.Max(0, newIncome);
        
        MsgCenter.SendMsg(MsgConst.MSG_INCOME_CHANGED, currentIncome, r);
    }

    public int GetStarRating()
    {
        if (currentLevel == null) 
        {
            Debug.LogWarning("[EconomyManager] 当前关卡未设置");
            return 0;
        }
        
        if (currentIncome >= currentLevel.star3Income) return 3;
        if (currentIncome >= currentLevel.star2Income) return 2;
        if (currentIncome >= currentLevel.star1Income) return 1;
        return 0;
    }

    // 获取当前关卡的评分标准（用于UI显示）
    public (int star1, int star2, int star3) GetLevelTargets()
    {
        if (currentLevel == null) return (0, 0, 0);
        return (currentLevel.star1Income, currentLevel.star2Income, currentLevel.star3Income);
    }

    // 获取当前收入占最高星级要求的百分比（用于进度条等）
    public float GetProgressPercentage()
    {
        if (currentLevel == null || currentLevel.star3Income == 0) return 0f;
        return Mathf.Clamp01((float)currentIncome / currentLevel.star3Income);
    }
}