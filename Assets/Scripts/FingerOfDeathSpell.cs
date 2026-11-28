using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly FingerOfDeathRaising fingerRaising=new FingerOfDeathRaising();
    readonly Queue<FingerOfDeathRaising.Pending> fingerReady=new Queue<FingerOfDeathRaising.Pending>();
    bool drainingFingerRaises;
    string FingerOfDeathError(bool player)
    {
        if(player?combat.PlayerDead:combat.Hp==0)return "需要仍存活的目标；倒地未死也可成为目标。";
        if(player?!CanSeeOwnBody():!PlayerSeesMonster)return "死亡一指需要看见目标。";
        if(!player&&map.DistanceFeet>60)return "目标超出60尺。";
        if(!player&&(map.IsContained(true)||map.IsContained(false)))return "吞咽造成内外隔离，当前不能看见该目标。";
        return "";
    }
    IEnumerator ResolveFingerOfDeath(bool player,int caster)
    {
        int target=player?0:-1;var failed=new HashSet<int>();
        long dc=8L+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]);
        yield return RollMappedAbilitySave(target,0,dc,failed,"死亡一指",2);
        int sum=0;var rolls=new int[7];for(int i=0;i<rolls.Length;i++){rolls[i]=Random.Range(1,9);sum+=rolls[i];}
        dice="死亡一指 · 7d8＋30死灵";yield return PresentRoll(8,rolls,"骰值 "+sum+" + 固定30 = "+FingerOfDeathRaising.Damage(sum,false));
        var reduction=new RayDamage();yield return ReduceRayDamage(true,FingerOfDeathRaising.Damage(sum,false),"死灵","死亡一指",reduction);
        int raw=failed.Contains(target)?reduction.Amount:reduction.Amount/2;
        int amount=combat.AdjustIncomingTypedDamage(player,raw,8);bool alreadyDead=player?combat.PlayerDead:combat.Hp==0;
        outcomeDeferDepth++;try{
            if(player)combat.ApplyPlayerDamage(amount,sourcePlayer:true);else combat.ApplyDamage(amount,sourcePlayer:true);
            ObserveDeaths();var corpse=player?playerCorpse:monsterCorpse;
            if(!alreadyDead&&(player?combat.PlayerDead:combat.Hp==0)&&fingerRaising.Schedule(target,corpse))history.Insert(0,"死亡一指杀死类人生物：将在施法者下回合开始尝试起尸；降至0HP但尚未死亡不会触发。");
            history.Insert(0,"死亡一指："+TransitActorName(target)+(failed.Contains(target)?"体质豁免失败":"体质豁免成功，伤害减半")+"，结算 "+amount+" 死灵伤害。");
            yield return MaintainConcentration(player?amount:0);
        }finally{outcomeDeferDepth--;CheckBattleOutcome();}
    }
    void StartFingerCasterTurn(){foreach(var pending in fingerRaising.StartTurn())fingerReady.Enqueue(pending);}
    IEnumerator DrainFingerRaising()
    {
        if(drainingFingerRaises||fingerReady.Count==0)yield break;drainingFingerRaises=true;bool busy=rolling;rolling=true;
        try{while(fingerReady.Count>0){
            var pending=fingerReady.Dequeue();var corpse=pending.Target==0?playerCorpse:monsterCorpse;
            ObserveDeaths();if(!FingerOfDeathRaising.SameCorpse(pending,corpse)||!map.BodyPresent(pending.Target)){history.Insert(0,"死亡一指起尸取消：原目标已复活或原遗体已被消耗。");continue;}
            if(corpse.Preserved(worldRounds)){history.Insert(0,"遗体防腐阻止死亡一指起尸，保留遗体。");continue;}
            if(map.IsContained(pending.Target==0)){
                int host=pending.Target==0?-1:0;var before=host==0?map.Player:map.Monster;int spaces=host==0?map.PlayerSpaces:map.MonsterSpaces;
                var plans=map.MandatoryReleasePlans();if(plans.Count==0||!map.ApplyMandatoryReleasePlan(plans[0])){history.Insert(0,"死亡一指起尸失败：无法按强制放出规则找到合法位置，保留遗体。");continue;}
                var after=host==0?map.Player:map.Monster;if(before!=after)ObserveGreaseMovement(host,before,after,spaces,spaces,true);
                history.Insert(0,"死亡一指：体内遗体已按最近合法位置强制放出，吞咽者仅作最短必要挪位。");
            }
            if(map.RescueCargoId==pending.Target){if(!map.ForceReleaseRescueCargo()){history.Insert(0,"死亡一指起尸失败：遗体无法合法放下。");continue;}rescueCarrier.Load.Clear();rescueDropMode=false;}
            Vector2Int p;int n;string name;if(!AnimateBodyInfo(pending.Target,out p,out n,out name)||!map.CanAnimateBody(new AnimatedUndeadRules(false).Create(nextSummonId),pending.Target)){history.Insert(0,"死亡一指起尸失败：遗体位置无法容纳独立僵尸。");continue;}
            if(CreateAnimatedFollower(pending.Target,false,7,true))history.Insert(0,"死亡一指：僵尸永久服从口头指令，不占用专注，不受操纵死尸24小时控制期限影响。");
        }
        RefreshUndeadFollowers();if(PendingTerrainCount>0)yield return DrainGreaseEntries();
        }finally{drainingFingerRaises=false;rolling=busy;}
    }
    void ResetFingerRaising(){fingerRaising.Clear();fingerReady.Clear();}
}
