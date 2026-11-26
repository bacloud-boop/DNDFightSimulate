using System.Collections;
using System.Collections.Generic;
public static class ThunderousSmiteRules
{
    public static int Dice(int slot,bool critical=false)=>slot>=1&&slot<=9?(slot+1)*(critical?2:1):0;
}
public sealed partial class TrainingGame
{
    IEnumerator ResolveThunderousSmiteHit(int target,SmiteHitResult hit)
    {
        if(!hit.ThunderPush||!BanishLiving(target))yield break;
        var failed=new HashSet<int>();yield return RollMappedAbilitySave(target,0,hit.ThunderDc,failed,"雷鸣斩",0);
        if(!failed.Contains(target)){history.Insert(0,"雷鸣斩：力量豁免成功，不推离、不倒地；附加雷鸣伤害不减半。");yield break;}
        // Player melee attacks currently target the training monster only.
        int pushed=map.ForceMoveAway(false,map.Player,map.PlayerSpaces,10);
        SetActorProne(target);SyncTimedBuffs();
        history.Insert(0,"雷鸣斩：力量豁免失败，推离"+pushed+"尺并倒地。不消耗移动力、不引发借机攻击、不追加碰撞伤害；300尺声响暂仅保留说明。");
        if(PendingTerrainCount>0)yield return DrainGreaseEntries();
    }
}
