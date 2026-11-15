using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
    sealed class MonsterSpellTurn {public bool Attempted,Cast,Bonus;}
    int MonsterTurnReach
    {
        get{var entry=monsterSpells.Next;var spell=entry==null?null:MonsterSpellCatalog.Find(entry.Name);return MonsterSpellReady&&spell!=null&&!spell.Healing?Mathf.Max(spell.Range,spell.EmanationRadius):MonsterAttackReach;}
    }
    bool MonsterSpellNeedsSight(DirectSpell spell)=>spell.Name=="Sacred Flame"||spell.Name=="Toll the Dead"||spell.Name=="Magic Missile"||spell.Name=="Blight"||spell.Name=="Power Word Kill";
    bool MonsterCanSeeTarget(int id)=>id==0&&MonsterSeesPlayer;
    int MonsterSpellTarget(DirectSpell spell)
    {
        if(spell.Healing)return -1;
        return !combat.PlayerDead&&!map.IsContained(true)&&map.DistanceFeet<=spell.Range&&(!MonsterSpellNeedsSight(spell)||MonsterSeesPlayer)?0:-2;
    }
    IEnumerator TryMonsterTurnSpell(MonsterSpellTurn result)
    {
        if(!combat.MonsterCanAct)yield break;
        var entry=monsterSpells.Take();if(entry==null)yield break;
        result.Attempted=true;
        var spell=MonsterSpellCatalog.Find(entry.Name);bool area=MonsterSpellCatalog.IsArea(spell);Vector2Int areaCenter=default(Vector2Int);
        int target=spell==null?-2:area?(TryPlanMonsterArea(spell,out areaCenter)?-3:-2):MonsterSpellTarget(spell);
        if(!MonsterSpellReady||spell==null||target==-2){history.Insert(0,"木桩本轮法术无法施放，改用自定义攻击；循环已移至下一项。");yield break;}
        result.Cast=true;result.Bonus=spell.Bonus;
        combat.MarkBattleStarted();
        history.Insert(0,"木桩施放"+spell.Chinese+"（"+(entry.Slot==0?"戏法":entry.Slot+"环")+"），消耗"+(spell.Bonus?"附赠动作":"动作")+"。");
        if(area){ShowMonsterArea(spell,areaCenter);monsterAreaChoosing=true;yield return new WaitForSecondsRealtime(fastAnimation?.2f:1f);monsterAreaChoosing=false;}
        var stopped=new SpellInterruption();yield return TryInterruptSpell(false,spell.Name,stopped);
        if(!stopped.Cancelled){
            if(area)yield return ResolveMonsterAreaSpell(spell,entry.Slot,areaCenter);
            else if(spell.Name=="Magic Missile")yield return MonsterMagicMissile(spell,entry.Slot,target);
            else if(spell.Name=="Scorching Ray")for(int n=0;n<spell.DiceCount(monsterSpells.CasterLevel,entry.Slot);n++){
                int victim=MonsterSpellTarget(spell);if(victim==-2)break;
                yield return MonsterNumericSpell(spell,entry.Slot,victim,2);
            }
            else yield return MonsterNumericSpell(spell,entry.Slot,target);
        }
        monsterAreaPreview=false;BreakOrdinaryInvisibility(false,"施法");CheckBattleOutcome();
    }
    IEnumerator MonsterMagicMissile(DirectSpell spell,int slot,int target)
    {
        if(target==0){yield return OfferShield(false);if(combat.ShieldActive){history.Insert(0,"护盾术阻挡木桩的魔法飞弹，伤害0。");yield break;}}
        int amount=0;for(int n=0;n<spell.DiceCount(monsterSpells.CasterLevel,slot);n++){
            int value=Random.Range(1,5);amount+=value+1;dice="木桩魔法飞弹 · 第"+(n+1)+"枚";yield return PresentRoll(4,new[]{value},value+" + 1，累计 "+amount);
        }
        yield return ApplyMonsterSpellDamage(target,amount,spell.DamageType,false);
    }
    IEnumerator MonsterNumericSpell(DirectSpell spell,int slot,int target,int fixedCount=0)
    {
        bool player=target==0;
        int distance=map.DistanceFeet;
        if(spell.Name=="Power Word Kill"&&combat.PlayerHp<=100){
            if(player)combat.KillPlayer();
            BreakOrdinaryInvisibility(false,"造成伤害或死亡效果");
            history.Insert(0,"木桩律令死亡：目标生命不超过100，触发直接死亡；防死结界按原有规则抵消。");yield break;
        }
        bool critical=false,half=false;
        if(spell.Attack){
            bool sees=MonsterCanSeeTarget(target),targetSees=PlayerSeesMonster;
            bool advantage=!targetSees||player&&(combat.PlayerHp==0||combat.PlayerParalyzed||combat.PlayerProne&&distance<=5);
            bool disadvantage=!sees||combat.MonsterPoisoned||combat.MonsterProne||combat.ConsumeMockery(false)||player&&BlurDisadvantage(false,true)||player&&combat.PlayerProne&&distance>5;
            if(player)advantage=combat.ConsumeGuidingLight(true)||advantage;
            if(!spell.Melee){
                if(combat.PlayerCanAct&&map.DistanceFeet<=5&&PlayerSeesMonster)disadvantage=true;
            }
            int first=Random.Range(1,21),second=advantage!=disadvantage?Random.Range(1,21):0,value=second==0?first:advantage?Mathf.Max(first,second):Mathf.Min(first,second);
            BreakOrdinaryInvisibility(false,"进行攻击检定");
            long bonus=MonsterSpellSequence.AttackBonus(combat.MonsterProficiencyBonus,EffectiveMonsterAbility(monsterSpells.Ability))+combat.MonsterRevivalPenalty;
            int bless=combat.MonsterBlessed?Random.Range(1,5):0,bane=combat.MonsterBaned?Random.Range(1,5):0,ward=player&&combat.PlayerBladeWard?Random.Range(1,5):0;
            long total=value+bonus+bless-bane-ward;int cover=0,armor=combat.PlayerArmorClass;
            mirrorIntercepted=false;
            if(player&&value!=1&&(value==20||total>=armor)){yield return ResolvePlayerHitDefense(value,total,sees,PerceivesWithSpecialSense(false,true));armor=combat.PlayerArmorClass+cover;}
            dice="木桩"+spell.Chinese+" · 命中";
            yield return PresentRoll(20,second==0?new[]{first}:new[]{first,second},"采用 "+value+" + "+bonus+" + 祝福 "+bless+" - 灾祸 "+bane+" - 剑刃防护 "+ward+" = "+total+" / AC "+armor);
            if(bless>0)yield return PresentRoll(4,new[]{bless},"祝福术 +"+bless);
            if(bane>0)yield return PresentRoll(4,new[]{bane},"灾祸术 -"+bane);
            if(ward>0)yield return PresentRoll(4,new[]{ward},"剑刃防护 -"+ward);
            if(mirrorIntercepted||value==1||value!=20&&total<armor){history.Insert(0,"木桩"+spell.Chinese+"未命中本体，伤害0。");yield break;}
            critical=value==20||player&&(combat.PlayerHp==0||combat.PlayerParalyzed)&&distance<=5;
        }
        if(spell.SaveAbility>=0){
            bool automatic=player&&(spell.SaveAbility==0||spell.SaveAbility==1)&&(combat.PlayerHp==0||combat.PlayerParalyzed);
            int modifier=PlayerSavingThrowModifier(spell.SaveAbility);
            int value=Random.Range(1,21);int[] shown=new[]{value};
            if(player){var adjustment=new SaveAdjustment();yield return ApplySaveAdjustments(true,automatic,adjustment);modifier+=adjustment.Value;}
            dice="木桩"+spell.Chinese+" · 目标"+PlayerAttributes.Names[spell.SaveAbility]+"豁免";
            if(!automatic)yield return PresentRoll(20,shown,value+" + "+modifier+" / DC "+MonsterSpellDc);
            bool success=!automatic&&(long)value+modifier>=MonsterSpellDc;
            if(success&&!spell.HalfSave){history.Insert(0,"目标通过豁免，"+spell.Chinese+"伤害0。");yield break;}half=success;
        }
        int sides=spell.Name=="Toll the Dead"&&(combat.PlayerHp<combat.PlayerMaxHp)?12:spell.Sides;
        int count=(fixedCount>0?fixedCount:spell.DiceCount(monsterSpells.CasterLevel,slot))*(critical?2:1),amount=0;int[] rolls=new int[count];
        for(int i=0;i<count;i++){rolls[i]=Random.Range(1,sides+1);amount+=rolls[i];}
        if(spell.Healing)amount=Mathf.Max(0,amount+AcidSplash.Modifier(EffectiveMonsterAbility(monsterSpells.Ability)));
        if(half)amount/=2;dice="木桩"+spell.Chinese+(critical?" · 暴击":"");yield return PresentRoll(sides,rolls,"结算 "+amount+(spell.Healing?" 治疗":" "+spell.DamageType));
        if(spell.Healing){history.Insert(0,"木桩"+spell.Chinese+"实际恢复 "+combat.HealMonster(amount)+" HP。");yield break;}
        int frost=player&&spell.Melee?AgathysRetaliation(true):0;
        yield return ApplyMonsterSpellDamage(target,amount,spell.DamageType,critical);
        if(frost>0)ApplyAgathysRetaliation(frost,false);
    }
    IEnumerator ApplyMonsterSpellDamage(int target,int raw,string type,bool critical)
    {
        if(target==0){int amount=combat.AdjustTypedDamage(true,raw,DamageTypes.Index(type));combat.ApplyPlayerDamage(amount,critical);history.Insert(0,"木桩法术对玩家造成 "+amount+" "+type+"伤害。");yield return MaintainConcentration(amount);}
    }
    IEnumerator FinishMonsterSpellTurn()
    {
        if(!combat.PlayerDead){yield return RepeatHoldPersonSave();combat.FinishMonsterTurnEffects();combat.EndRound();fighter.EndRound();lightAttacksThisTurn.Clear();map.RefreshMovement();tacticalMovement=false;RefreshConcentration();yield return PlayerDeathSave();}
        rolling=false;
    }
}
