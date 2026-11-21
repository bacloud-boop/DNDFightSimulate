using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly WardingBondEffect wardingBond=new WardingBondEffect();
    readonly Queue<int> bondConcentration=new Queue<int>();
    bool drainingBond,bondWilling;int bondTarget=-1;
    void InitializeWardingBond()
    {
        combat.WardBonus=player=>BondBonus(player?0:-1);combat.UniversalResistance=player=>BondBonus(player?0:-1)>0;
        combat.DamageApplied=(player,packet)=>TransferBondDamage(player?0:-1,packet);
        rescueCarrier.WardBonus=()=>BondBonus(BattleMap.RescueId);
        rescueCarrier.ExtraResistance=type=>BondBonus(BattleMap.RescueId)>0||type==5&&map.RescueAquatic.Submerged;
        rescueCarrier.DamageApplied=(amount,type)=>TransferBondSingle(BattleMap.RescueId,amount,type);
    }
    void HookBondSummon(SummonedUnit unit)
    {
        unit.WardBonus=()=>BondBonus(unit.Id);unit.DamageApplied=packet=>TransferBondDamage(unit.Id,packet);
    }
    int BondBonus(int actor)
    {
        if(!wardingBond.Active(worldRounds))return 0;
        Vector2Int target;int n;
        if(!map.MovementBody(wardingBond.Target,out target,out n)||!wardingBond.Validate(worldRounds,combat.PlayerHp>0&&!combat.PlayerDead,BattleMap.BodyDistance(map.Player,map.PlayerSpaces,target,n)))return 0;
        return wardingBond.Protects(actor,worldRounds)?1:0;
    }
    void TransferBondSingle(int actor,int amount,int type){var packet=new long[14];packet[type>=0&&type<13?type:13]=amount;TransferBondDamage(actor,packet);}
    void TransferBondDamage(int actor,long[] packet)
    {
        // Evaluate the triggering event even if it just killed the protected unit.
        if(actor==0&&combat.PlayerHp<=0){wardingBond.Clear();return;}
        if(!wardingBond.Protects(actor,worldRounds)||combat.PlayerHp<=0||combat.PlayerDead)return;
        var resolved=combat.ResolveTypedPacket(true,packet);int amount=0;foreach(long value in resolved)amount=(int)System.Math.Min(int.MaxValue,(long)amount+value);
        if(amount<=0)return;
        if(resolved.Length>5&&resolved[5]>0)combat.FireExposure?.Invoke(true);
        combat.ApplyPlayerDamage(amount,sourcePlayer:null,resolvedTypes:resolved);
        history.Insert(0,"守护之链：保留原伤害类型，施法者自身抗性／免疫结算后承受 "+amount+" 伤害（临时生命优先吸收）。");
        bondConcentration.Enqueue(amount);RefreshConcentration();NoticeCastingInterruption();if(combat.PlayerHp==0)wardingBond.Clear();
    }
    IEnumerator DrainBondConcentration()
    {
        if(drainingBond)yield break;drainingBond=true;bool busy=rolling;rolling=true;
        try{while(bondConcentration.Count>0){int damage=bondConcentration.Dequeue();yield return MaintainConcentrationCore(damage);}CheckBattleOutcome();}
        finally{rolling=busy;drainingBond=false;}
    }
    string WardingBondError()
    {
        if(!bondWilling)return "请确认目标自愿接受守护之链。";
        if(bondTarget==0||!HealingHandsCanTouch(0,bondTarget))return "必须接触另一名存活生物（5尺内），不能选择自身。";
        return worldRounds<0||worldRounds>long.MaxValue-600?"世界时间超出范围。":"";
    }
    void DrawWardingBondChoices()
    {
        foreach(int id in BreathActors())if(id!=0){Vector2Int p;int n;string name;if(HealingHandsTarget(id,out p,out n,out name)&&GUILayout.Button((bondTarget==id?"✓ ":"")+name,button))bondTarget=id;}
        bondWilling=GUILayout.Toggle(bondWilling,"确认目标自愿接受守护之链",label);
        GUILayout.Label("目标AC与所有豁免+1、全伤害抗性。转移伤害保留类型，再结算施法者的抗性／免疫；目标临时生命吸收的伤害也会转移。非专注1小时；施法者0HP、距离超过60尺或重施时结束。",small);
    }
    void CastWardingBond(int target)
    {
        if(target==0||!HealingHandsCanTouch(0,target)||!wardingBond.Begin(target,worldRounds)){history.Insert(0,"守护之链目标失效，施法动作已消耗。");return;}
        history.Insert(0,"守护之链已连接玩家与"+TransitActorName(target)+"；非专注1小时，60尺内有效。");
    }
}
