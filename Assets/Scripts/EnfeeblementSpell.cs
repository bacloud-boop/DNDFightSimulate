using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly EnfeeblementEffect enfeeblement=new EnfeeblementEffect();
    sealed class EnfeeblementSaveRoll {public int Value;public int[] Dice;}
    EnfeeblementSaveRoll RollEnfeeblementSave(bool player,int ability,bool disadvantage=false){
        disadvantage|=SizeStrengthDisadvantage(player,ability)||RayStrengthDisadvantage(player,ability);bool advantage=SizeStrengthAdvantage(player,ability);int first=Random.Range(1,21),second=advantage!=disadvantage?Random.Range(1,21):0;
        return new EnfeeblementSaveRoll{Value=second>0?(advantage?Mathf.Max(first,second):Mathf.Min(first,second)):first,Dice=second>0?new[]{first,second}:new[]{first}};
    }
    bool RayWeakened(bool player)=>enfeeblement.Weakened(player,worldRounds,combat.Round);
    bool RayStrengthDisadvantage(bool player,int ability)=>enfeeblement.StrengthDisadvantage(player,ability,worldRounds,combat.Round);
    bool RayAttackDisadvantage(bool player,int ability){
        bool result=enfeeblement.ConsumeAttack(player,ability,worldRounds,combat.Round);
        bool mockery=combat.ConsumeMockery(player);
        if(result&&concentrationSpell=="衰弱射线"&&!enfeeblement.Active(true,worldRounds,combat.Round)&&!enfeeblement.Active(false,worldRounds,combat.Round))EndConcentration("下次攻击劣势已使用");
        return result||mockery;
    }
    int MonsterCustomAttackAbility=>MonsterPolymorphed?BeastAttack.Find(polymorph.Form).Ability:monsterAttackAbility-1;
    string EnfeeblementError(bool player)
    {
        if(player?combat.PlayerDead:combat.Hp==0)return "衰弱射线不能指定死亡目标。";
        if(!player&&map.DistanceFeet>60)return "目标超出60尺射程。";
        return enfeeblement.CanCast(worldRounds)?"":"世界时间超出可记录范围。";
    }
    IEnumerator ResolveEnfeeblement(bool player,int caster)
    {
        long dc=8L+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]);
        var roll=RollEnfeeblementSave(player,2);int modifier=player?PlayerSavingThrowModifier(2):MonsterSavingThrowModifier(2);
        var adjustment=new SaveAdjustment();yield return ApplySaveAdjustments(player,false,adjustment);modifier+=adjustment.Value;
        dice="衰弱射线 · "+(player?"玩家":"木桩")+"体质豁免";
        yield return PresentRoll(20,roll.Dice,roll.Value+" + "+modifier+" / DC "+dc);
        bool failed=(long)roll.Value+modifier<dc;
        enfeeblement.Cast(player,failed,dc,worldRounds,combat.Round);
        concentrationSpell="衰弱射线";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=worldRounds+10;
        history.Insert(0,"衰弱射线："+(player?"玩家":"木桩")+(failed?"豁免失败，力量类D20检定劣势；整次命中的伤害合并减1d8，回合末重复体质豁免。":"豁免成功，仅下次攻击劣势，最迟至施法者下回合开始。"));
    }
    IEnumerator RepeatEnfeeblementSave(bool player)
    {
        if(!RayWeakened(player)||(player?combat.PlayerDead:combat.Hp==0))yield break;
        var roll=RollEnfeeblementSave(player,2);int modifier=player?PlayerSavingThrowModifier(2):MonsterSavingThrowModifier(2);
        var adjustment=new SaveAdjustment();yield return ApplySaveAdjustments(player,false,adjustment);modifier+=adjustment.Value;
        dice="衰弱射线 · 回合末体质豁免";
        yield return PresentRoll(20,roll.Dice,roll.Value+" + "+modifier+" / DC "+enfeeblement.Dc);
        if((long)roll.Value+modifier>=enfeeblement.Dc)EndConcentration("目标通过衰弱射线重复豁免");
        else history.Insert(0,"衰弱射线重复豁免失败，继续生效。");
    }
    sealed class RayDamage {public int Amount;public long[] Raw;}
    IEnumerator ReduceRayPacket(bool sourcePlayer,long[] raw,bool rolled,string context,RayDamage result)
    {
        result.Raw=(long[])raw.Clone();
        if(!rolled||!RayWeakened(sourcePlayer))yield break;
        int penalty=Random.Range(1,9);dice="衰弱射线 · 伤害减值";
        yield return PresentRoll(8,new[]{penalty},context+"：本次合并伤害减 "+penalty+"，暴击不翻倍减值骰");
        var allocation=new DamageReductionResult();yield return AllocateDamageReduction(raw,penalty,context,allocation);
        result.Raw=allocation.Raw;
        history.Insert(0,"衰弱射线："+context+"抵扣最多 "+penalty+"，分类伤害最低0；随后结算抗性与免疫。");
    }
    IEnumerator ReduceRayDamage(bool sourcePlayer,int amount,string type,string context,RayDamage result)
    {
        var raw=new long[DamageTypes.Names.Length];raw[DamageTypes.Index(type)]=amount;
        yield return ReduceRayPacket(sourcePlayer,raw,true,context,result);
        result.Amount=DamageTypes.Total(DamageTypes.Resolve(result.Raw));
    }
    int ResolveRayPacket(bool targetPlayer,long[] raw)
    {
        return combat.AdjustDamagePacket(targetPlayer,raw);
    }
}
