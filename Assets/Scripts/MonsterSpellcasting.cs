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
    bool MonsterCanSeeTarget(int id)
    {
        if(id==0)return MonsterSeesPlayer;var unit=FindSummon(id);
        return unit!=null&&unit.Alive&&VisibilityBetween(false,map.Monster.x,map.Monster.y,map.MonsterSpaces,unit.X,unit.Y,unit.Spaces)<2;
    }
    int MonsterSpellTarget(DirectSpell spell)
    {
        if(spell.Healing)return -1;
        int target=-2,best=int.MaxValue;
        if(!combat.PlayerDead&&!map.IsContained(true)&&map.DistanceFeet<=spell.Range&&(!MonsterSpellNeedsSight(spell)||MonsterSeesPlayer)){target=0;best=map.DistanceFeet;}
        foreach(var unit in map.Summons)if(unit.Alive){int distance=BattleMap.BodyDistance(map.Monster,map.MonsterSpaces,new Vector2Int(unit.X,unit.Y),unit.Spaces);if(distance<=spell.Range&&distance<best&&(!MonsterSpellNeedsSight(spell)||MonsterCanSeeTarget(unit.Id))){target=unit.Id;best=distance;}}
        return target;
    }
    IEnumerator TryMonsterTurnSpell(MonsterSpellTurn result)
    {
        if(!combat.MonsterCanAct)yield break;
        var entry=monsterSpells.Take();if(entry==null)yield break;
        result.Attempted=true;
        var spell=MonsterSpellCatalog.Find(entry.Name);bool area=MonsterSpellCatalog.IsArea(spell);Vector2Int areaCenter=default(Vector2Int);
        int target=spell==null?-2:area?(TryPlanMonsterArea(spell,out areaCenter)?-3:-2):MonsterSpellTarget(spell);
        if(!MonsterSpellReady||spell==null||target==-2||!SpellVoiceAllowed(false,spell.Name)){history.Insert(0,"木桩本轮法术无法施放，改用自定义攻击；循环已移至下一项。");yield break;}
        result.Cast=true;result.Bonus=spell.Bonus;
        combat.MarkBattleStarted();
        history.Insert(0,"木桩施放"+spell.Chinese+"（"+(entry.Slot==0?"戏法":entry.Slot+"环")+"），消耗"+(spell.Bonus?"附赠动作":"动作")+"。");
        if(area){ShowMonsterArea(spell,areaCenter);monsterAreaChoosing=true;yield return new WaitForSecondsRealtime(fastAnimation?.2f:1f);monsterAreaChoosing=false;}
        var info=SpellInfo(spell.Name);
        if(info!=null&&System.Array.IndexOf(info.components,"声音")>=0){
            ObserveWaterBreathing();
            if(map.MonsterAquatic.Submerged&&!monsterBreathing.CanBreathe){monsterBreathing.Observe(false,worldRounds,EffectiveMonsterAbility(2),true);history.Insert(0,"木桩水下声音施法，立即结束屏息并呛水。");}
        }
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
        monsterAreaPreview=false;BreakOrdinaryInvisibility(false,"施法");RefreshBestialSpirit();CheckBattleOutcome();
    }
    IEnumerator MonsterMagicMissile(DirectSpell spell,int slot,int target)
    {
        if(target==0){yield return OfferShield(false);if(combat.ShieldActive){history.Insert(0,"护盾术阻挡木桩的魔法飞弹，伤害0。");yield break;}}
        int amount=0;for(int n=0;n<spell.DiceCount(monsterSpells.CasterLevel,slot);n++){
            int value=Random.Range(1,5);dice="木桩魔法飞弹 · 第"+(n+1)+"枚";yield return PresentRoll(4,new[]{value},value+" + 1");
            var reduced=new RayDamage();yield return ReduceRayDamage(false,value+1,"力场","木桩魔法飞弹第"+(n+1)+"枚",reduced);amount+=reduced.Amount;
        }
        yield return ApplyMonsterSpellDamage(target,amount,spell.DamageType,false);
    }
    IEnumerator MonsterNumericSpell(DirectSpell spell,int slot,int target,int fixedCount=0)
    {
        bool player=target==0;var unit=target>0?FindSummon(target):null;
        int distance=player?map.DistanceFeet:unit==null?0:BattleMap.BodyDistance(map.Monster,map.MonsterSpaces,new Vector2Int(unit.X,unit.Y),unit.Spaces);
        if(spell.Name=="Power Word Kill"&&(player?combat.PlayerHp:unit.Hp)<=100){
            if(player)combat.KillPlayer();else unit.Damage(unit.Hp);
            BreakOrdinaryInvisibility(false,"造成伤害或死亡效果");
            history.Insert(0,"木桩律令死亡：目标生命不超过100，触发直接死亡；防死结界按原有规则抵消。");yield break;
        }
        bool critical=false,half=false;
        if(spell.Attack){
            bool sees=MonsterCanSeeTarget(target),targetSees=player?PlayerSeesMonster:SpiritSees(unit,-1,map.Monster,map.MonsterSpaces);
            bool advantage=unit!=null&&unit.Prone&&distance<=5||player&&Entangled(true)||unit!=null&&unit.Restrained||!targetSees||player&&(combat.PlayerHp==0||combat.PlayerParalyzed||combat.PlayerProne&&distance<=5);
            bool disadvantage=FearDisadvantage(false)||Entangled(false)|RayAttackDisadvantage(false,monsterSpells.Ability)||unit!=null&&unit.Prone&&distance>5||!sees||combat.MonsterPoisoned||combat.MonsterProne||player&&BlurDisadvantage(false,true)||player&&combat.PlayerProne&&distance>5||unit!=null&&unit.Dodging&&targetSees&&unit.EffectiveSpeed>0;
            if(player)advantage=combat.ConsumeGuidingLight(true)||advantage;
            if(!spell.Melee){
                if(combat.PlayerCanAct&&map.DistanceFeet<=5&&PlayerSeesMonster)disadvantage=true;
                foreach(var ally in map.Summons)if(ally.Alive&&BattleMap.BodyDistance(map.Monster,map.MonsterSpaces,new Vector2Int(ally.X,ally.Y),ally.Spaces)<=5&&SpiritSees(ally,-1,map.Monster,map.MonsterSpaces))disadvantage=true;
            }
            int first=Random.Range(1,21),second=advantage!=disadvantage?Random.Range(1,21):0,value=second==0?first:advantage?Mathf.Max(first,second):Mathf.Min(first,second);
            BreakOrdinaryInvisibility(false,"进行攻击检定");
            long bonus=MonsterSpellSequence.AttackBonus(combat.MonsterProficiencyBonus,EffectiveMonsterAbility(monsterSpells.Ability))+combat.MonsterRevivalPenalty+monsterBreathing.D20Penalty;
            int bless=combat.MonsterBlessed?Random.Range(1,5):0,bane=combat.MonsterBaned?Random.Range(1,5):0,ward=player&&combat.PlayerBladeWard?Random.Range(1,5):0;
            long total=value+bonus+bless-bane-ward;int cover=map.CreatureCoverBonus(-1,target),armor=(player?combat.PlayerArmorClass:unit.Armor)+cover;
            mirrorIntercepted=false;
            if(player&&value!=1&&(value==20||total>=armor)){yield return ResolvePlayerHitDefense(value,total,sees,PerceivesWithSpecialSense(false,true),cover);armor=combat.PlayerArmorClass+cover;}
            dice="木桩"+spell.Chinese+" · 命中";
            yield return PresentRoll(20,second==0?new[]{first}:new[]{first,second},"采用 "+value+" + "+bonus+" + 祝福 "+bless+" - 灾祸 "+bane+" - 剑刃防护 "+ward+" = "+total+" / AC "+armor);
            if(bless>0)yield return PresentRoll(4,new[]{bless},"祝福术 +"+bless);
            if(bane>0)yield return PresentRoll(4,new[]{bane},"灾祸术 -"+bane);
            if(ward>0)yield return PresentRoll(4,new[]{ward},"剑刃防护 -"+ward);
            if(mirrorIntercepted||value==1||value!=20&&total<armor){history.Insert(0,"木桩"+spell.Chinese+"未命中本体，伤害0。");yield break;}
            critical=value==20||player&&(combat.PlayerHp==0||combat.PlayerParalyzed)&&distance<=5;
        }
        if(spell.SaveAbility>=0){
            bool automatic=(spell.SaveAbility==0||spell.SaveAbility==1)&&(player?combat.PlayerHp==0||combat.PlayerParalyzed:unit.SleepStage==2);
            int modifier=player?PlayerSavingThrowModifier(spell.SaveAbility):AcidSplash.Modifier(SummonAbility(unit,spell.SaveAbility))+unit.Breathing.D20Penalty;
            bool dodge=!player&&spell.SaveAbility==1&&unit.Dodging&&unit.EffectiveSpeed>0;
            var roll=player?RollArmorSave(true,spell.SaveAbility):new ArmorSaveRoll{Value=Random.Range(1,21)};
            int value=roll.Value;int[] shown=player?roll.Dice:new[]{value};
            if(dodge){int second=Random.Range(1,21);value=Mathf.Max(value,second);shown=new[]{roll.Value,second};}
            if(!player&&spell.SaveAbility==1&&unit.Restrained){int second=Random.Range(1,21);value=Mathf.Min(value,second);shown=new[]{roll.Value,second};}
            if(player){var adjustment=new SaveAdjustment();yield return ApplySaveAdjustments(true,automatic,adjustment,spell.SaveAbility,map.Monster,spell.Name=="Sacred Flame",map.MonsterSpaces);modifier+=adjustment.Value;}
            else if(spell.SaveAbility==1&&spell.Name!="Sacred Flame")modifier+=map.CreatureCoverBonus(-1,target);
            dice="木桩"+spell.Chinese+" · 目标"+PlayerAttributes.Names[spell.SaveAbility]+"豁免";
            if(!automatic)yield return PresentRoll(20,shown,value+" + "+modifier+" / DC "+MonsterSpellDc);
            bool success=!automatic&&(long)value+modifier>=MonsterSpellDc;
            if(success&&!spell.HalfSave){history.Insert(0,"目标通过豁免，"+spell.Chinese+"伤害0。");yield break;}half=success;
        }
        int sides=spell.Name=="Toll the Dead"&&(player?combat.PlayerHp<combat.PlayerMaxHp:unit.Hp<unit.MaxHp)?12:spell.Sides;
        int count=(fixedCount>0?fixedCount:spell.DiceCount(monsterSpells.CasterLevel,slot))*(critical?2:1),amount=0;int[] rolls=new int[count];
        for(int i=0;i<count;i++){rolls[i]=Random.Range(1,sides+1);amount+=rolls[i];}
        if(spell.Healing)amount=Mathf.Max(0,amount+AcidSplash.Modifier(EffectiveMonsterAbility(monsterSpells.Ability)));
        dice="木桩"+spell.Chinese+(critical?" · 暴击":"");yield return PresentRoll(sides,rolls,"结算 "+amount+(spell.Healing?" 治疗":" "+spell.DamageType));
        if(spell.Healing){history.Insert(0,"木桩"+spell.Chinese+"实际恢复 "+combat.HealMonster(amount)+" HP。");yield break;}
        var rayDamage=new RayDamage();yield return ReduceRayDamage(false,amount,spell.DamageType,"木桩法术",rayDamage);amount=rayDamage.Amount;
        if(half)amount/=2;
        int frost=player&&spell.Melee?AgathysRetaliation(true):0;
        yield return ApplyMonsterSpellDamage(target,amount,spell.DamageType,critical);
        if(frost>0)ApplyAgathysRetaliation(frost,false);
    }
    IEnumerator ApplyMonsterSpellDamage(int target,int raw,string type,bool critical)
    {
        if(target==0){int amount=combat.AdjustTypedDamage(true,raw,DamageTypes.Index(type));combat.ApplyPlayerDamage(amount,critical);history.Insert(0,"木桩法术对玩家造成 "+amount+" "+type+"伤害。");yield return MaintainConcentration(amount);}
        else {var unit=FindSummon(target);if(unit==null||!unit.Alive)yield break;int amount=SilenceUnitImmune(unit,type)?0:unit.Aquatic.Submerged&&type=="火焰"?raw/2:raw;unit.Damage(amount,DamageTypes.Index(type));history.Insert(0,"木桩法术对"+unit.Name+"造成 "+amount+" "+type+"伤害。");BreakOrdinaryInvisibility(false,"造成伤害");}
    }
    IEnumerator FinishMonsterSpellTurn()
    {
        if(!combat.PlayerDead){yield return RepeatHoldPersonSave();FinishWaterBreathingTurn(false);yield return RepeatSensorySaves(false);combat.FinishMonsterTurnEffects();combat.EndRound();fighter.EndRound();lightAttacksThisTurn.Clear();map.RefreshMovement();tacticalMovement=false;RefreshConcentration();yield return PlayerDeathSave();}
        rolling=false;
    }
}
