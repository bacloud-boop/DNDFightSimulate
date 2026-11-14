using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
    bool outcomeRecorded;int outcomeDeferDepth;
    long stableRecoveryAt=-1;
    void UpdateStableRecovery(){if(combat.PlayerStable&&stableRecoveryAt>=0&&worldRounds>=stableRecoveryAt){combat.HealPlayer(1);stableRecoveryAt=-1;history.Insert(0,"稳定休息后恢复1生命，苏醒。");}}
    void CheckBattleOutcome()
    {
        if(outcomeDeferDepth>0||!combat.BattleFinished||outcomeRecorded)return;
        outcomeRecorded=true;history.Insert(0,combat.PlayerDead&&combat.Hp==0?"双方死亡。战斗结束，保留现场，可手动重置。":combat.PlayerDead?"玩家死亡。战斗结束，保留现场，可手动重置。":"木桩死亡。战斗结束，保留现场，可手动重置。");
    }
    IEnumerator PlayerDeathSave()
    {
        if(!combat.NeedsDeathSave)yield break;
        int value=Random.Range(1,21);dice="玩家回合开始 · 死亡豁免";
        var adjustment=new SaveAdjustment();yield return ApplySaveAdjustments(true,false,adjustment);
        yield return PresentRoll(20,new[]{value},value==20?"自然20：恢复1生命":value==1?"自然1：累计2次失败":value+adjustment.Value>=10?"累计1次成功":"累计1次失败");
        combat.ResolveDeathSave(value,adjustment.Value);
        history.Insert(0,"死亡豁免 D20="+value+"，"+combat.PlayerLifeLabel+"；成功 "+combat.DeathSuccesses+" / 3，失败 "+combat.DeathFailures+" / 3。");
        if(combat.PlayerStable){int hours=Random.Range(1,5);dice="稳定后自然恢复时间";yield return PresentRoll(4,new[]{hours},hours+"小时后恢复1生命");stableRecoveryAt=worldRounds+hours*600;}
        CheckBattleOutcome();
    }
}
