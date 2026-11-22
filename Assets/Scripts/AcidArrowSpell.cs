using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly AcidArrowResidue acidArrows=new AcidArrowResidue();int acidArrowTarget=-1;
    bool AcidArrowIsolated(int source,int target)=>AcidArrowResidue.CrossesBoundary(source,target,map.IsContained(true),map.IsContained(false));
    bool AcidArrowTargetAvailable(int actor)
    {
        if(actor==0)return !combat.PlayerDead&&map.BodyPresent(0);if(actor==-1)return combat.Hp>0&&map.BodyPresent(-1);
        if(actor==BattleMap.RescueId)return !rescueCarrier.Dead&&map.RescuePosition.HasValue&&map.BodyPresent(actor);
        var unit=FindSummon(actor);return unit!=null&&unit.Alive;
    }
    string AcidArrowError()
    {
        Vector2Int p;int n;string name;if(!AcidArrowTargetAvailable(acidArrowTarget)||!SummonTarget(acidArrowTarget,out p,out n,out name))return "请选择仍存活的玩家、木桩或召唤物。";
        if(BattleMap.BodyDistance(map.Player,map.PlayerSpaces,p,n)>90)return "目标超出90尺。";
        if(AcidArrowIsolated(0,acidArrowTarget))return "不能隔着吞咽边界攻击。";
        return acidArrows.CanAdd(acidArrowTarget)?"":"残留效果编号或回合计数超出范围。";
    }
    void DrawAcidArrowChoices()
    {
        foreach(int actor in BreathActors())if(actor!=BattleMap.RescueId&&GUILayout.Button((acidArrowTarget==actor?"✓ ":"")+TransitActorName(actor),button))acidArrowTarget=actor;
        GUILayout.Label("命中 "+AcidArrowResidue.InitialDice(directSlotLevel)+"d4强酸；目标下个回合末另掷 "+AcidArrowResidue.LaterDice(directSlotLevel)+"d4。未命中仅初始伤害减半，不留残留。暴击只翻倍初始骰；多次命中的残留独立计时、分别结算，无需专注。",small);
    }
    IEnumerator ResolveAcidArrow(int source,int caster,int slot,int target)
    {
        if(AcidArrowResidue.InitialDice(slot)==0||!AcidArrowTargetAvailable(target)||!acidArrows.CanAdd(target))yield break;
        Vector2Int from,to;int n,m;string name;if(AcidArrowIsolated(source,target)||!CommandBody(source,out from,out n)||!SummonTarget(target,out to,out m,out name)||BattleMap.BodyDistance(from,n,to,m)>90)yield break;
        var attack=new SpellAttackResult();yield return RollMappedSpellAttack(source,target,caster,"马友夫强酸箭",attack);
        if(attack.Intercepted){history.Insert(0,"马友夫强酸箭命中镜像而非本体，未对本体生成初始或残留伤害。");yield break;}
        int count=AcidArrowResidue.InitialDice(slot,attack.Critical),sum=0;var rolls=new int[count];for(int i=0;i<count;i++){rolls[i]=Random.Range(1,5);sum+=rolls[i];}
        dice="马友夫强酸箭 · 初始强酸";yield return PresentRoll(4,rolls,count+"d4 = "+sum+(attack.Hit?attack.Critical?"（暴击）":"":"，未命中仅溅射半伤"));
        var packet=new long[13];packet[3]=sum;
        if(attack.Hit&&source==0&&(target==0||target==-1)){var extra=new ExtraMarkDamage();yield return RollMarkDamage(target==0,attack.Critical,extra);if(extra.Raw>0)packet[mark.DamageType]+=extra.Raw;}
        var reduced=new RayDamage();yield return ReduceRayPacket(source==0,packet,true,"马友夫强酸箭初始伤害",reduced);
        if(!attack.Hit)for(int i=0;i<reduced.Raw.Length;i++)reduced.Raw[i]/=2;
        outcomeDeferDepth++;try{
            int amount=ApplyMappedDamagePacket(target,source,reduced.Raw,attack.Critical);
            if(attack.Hit&&AcidArrowTargetAvailable(target)){var entry=acidArrows.Add(source,target,slot);if(entry!=null)history.Insert(0,entry.Title+"残留 #"+entry.Id+"："+TransitActorName(target)+"下个回合末结算 "+entry.Dice+"d4强酸，不覆盖已有残留。");}
            history.Insert(0,"马友夫强酸箭："+TransitActorName(target)+(attack.Hit?"命中":"未命中，初始半伤且无残留")+"，本次结算 "+amount+" 点伤害。");
            yield return MaintainConcentration(target==0?amount:0);RefreshUndeadFollowers();RefreshBestialSpirit();ReconcileFallenRescue();ClearUnavailableAcidArrows();
        }finally{outcomeDeferDepth--;CheckBattleOutcome();}
    }
    void ClearUnavailableAcidArrows()=>acidArrows.RemoveUnavailable(AcidArrowTargetAvailable);
    IEnumerator ResolveAcidArrowResidue(AcidArrowResidue.Entry entry)
    {
        if(!acidArrows.Take(entry.Id)||!AcidArrowTargetAvailable(entry.Target))yield break;
        int sum=0;var rolls=new int[entry.Dice];for(int i=0;i<rolls.Length;i++){rolls[i]=Random.Range(1,5);sum+=rolls[i];}
        dice=entry.Title+"残留 #"+entry.Id;yield return PresentRoll(4,rolls,entry.Dice+"d4 = "+sum+" 强酸；不进行攻击检定，不受初次暴击影响");
        var reduced=new RayDamage();yield return ReduceRayDamage(entry.Source==0,sum,"强酸",entry.Title+"残留 #"+entry.Id,reduced);
        var packet=new long[13];packet[3]=reduced.Amount;outcomeDeferDepth++;
        try{
            int amount=ApplyMappedDamagePacket(entry.Target,entry.Source,packet);history.Insert(0,entry.Title+"残留 #"+entry.Id+"："+TransitActorName(entry.Target)+"结算 "+amount+" 强酸伤害，本份残留结束。");
            yield return MaintainConcentration(entry.Target==0?amount:0);RefreshUndeadFollowers();RefreshBestialSpirit();ReconcileFallenRescue();ClearUnavailableAcidArrows();
        }finally{outcomeDeferDepth--;CheckBattleOutcome();}
    }
}
