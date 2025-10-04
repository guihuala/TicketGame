using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public int currentIncome { get; private set; }

    public void ResetIncome() => currentIncome = 0;

    public void ApplyResult(CheckResult r)
    {
        currentIncome += r.incomeDelta;
        MsgCenter.SendMsg(MsgConst.MSG_INCOME_CHANGED, currentIncome, r);
    }

    public int GetStar(DaySchedule day)
    {
        if (day == null) return 0;
        if (currentIncome >= day.star3Income) return 3;
        if (currentIncome >= day.star2Income) return 2;
        if (currentIncome >= day.star1Income) return 1;
        return 0;
    }
}
