using System;
using System.Collections;
using UnityEngine;
public sealed partial class TrainingGame
{
    sealed class SpellAttackResult {public bool Hit,Critical,Intercepted;}
    IEnumerator RollMappedSpellAttack(int source,int target,int caster,string title,SpellAttackResult result,bool melee=false,Vector2Int? effectOrigin=null,int effectOriginSpaces=1,int? effectSource=null)
    {
        Vector2Int origin,p;int n,m;string name;if(!CommandBody(source,out origin,out n))yield break;
        if(effectOrigin.HasValue?!HealingHandsTarget(target,out p,out m,out name):!SummonTarget(target,out p,out m,out name))yield break;
        bool playerSource=source==0,primary=target==0||target==-1,player=target==0;var unit=primary?null:FindSummon(target);
        bool sees=playerSource?(primary?Visibility(true,player)<2:VisibilityAt(true,p,m)<2):MonsterCanSeeTarget(target);
        bool targetSees=primary?Visibility(player,playerSource)<2:SpiritSees(unit,source,origin,n);
        int distance=BattleMap.BodyDistance(origin,n,p,m);
        bool helpless=primary?(player?combat.PlayerHp==0||combat.PlayerParalyzed:combat.MonsterHelpless):unit.SleepStage==2;
        bool prone=primary?(player?combat.PlayerProne:combat.MonsterProne):unit.Prone,restrained=primary?Entangled(player):unit.Restrained;
        bool advantage=FaerieAdvantage(target,sees)|!targetSees|helpless|(prone&&distance<=5)|restrained;
        if(playerSource)advantage|=fighter.ConsumeAdvantage(classes[0]);if(primary)advantage|=combat.ConsumeGuidingLight(player);
        bool disadvantage=(!sees||prone&&distance>5)|FearDisadvantage(playerSource)|Entangled(playerSource)|RayAttackDisadvantage(playerSource,playerSource?SpellAbilityIndex(caster):monsterSpells.Ability);
        disadvantage|=playerSource?combat.PlayerPoisoned||combat.PlayerProne||ResurrectionD20Disadvantage(true):combat.MonsterPoisoned||combat.MonsterProne;
        if(player)disadvantage|=BlurDisadvantage(playerSource,true);
        if(unit!=null)disadvantage|=unit.Dodging&&unit.EffectiveSpeed>0&&targetSees;
        if(!melee&&playerSource)disadvantage|=combat.MonsterCanAct&&MonsterSeesPlayer&&map.DistanceFeet<=5;
        else if(!melee){
            disadvantage|=combat.PlayerCanAct&&PlayerSeesMonster&&map.DistanceFeet<=5;
            foreach(var ally in map.SceneSummons)if(ally.Alive&&ally.ControllerReady&&!ally.FeignDeath&&ally.SleepStage==0&&BattleMap.BodyDistance(origin,n,new Vector2Int(ally.X,ally.Y),ally.Spaces)<=5&&SpiritSees(ally,source,origin,n))disadvantage=true;
        }
        int first=UnityEngine.Random.Range(1,21),second=advantage!=disadvantage?UnityEngine.Random.Range(1,21):0,value=second==0?first:advantage?Math.Max(first,second):Math.Min(first,second);
        int bless=(playerSource?combat.PlayerBlessed:combat.MonsterBlessed)?UnityEngine.Random.Range(1,5):0,bane=(playerSource?combat.PlayerBaned:combat.MonsterBaned)?UnityEngine.Random.Range(1,5):0,ward=player&&combat.PlayerBladeWard?UnityEngine.Random.Range(1,5):0;
        long bonus=(playerSource?(long)classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)])+PlayerAquaticD20Penalty:MonsterSpellSequence.AttackBonus(combat.MonsterProficiencyBonus,EffectiveMonsterAbility(monsterSpells.Ability))+combat.MonsterRevivalPenalty+monsterBreathing.D20Penalty)+bless-bane-ward;
        // Conditions keyed to the attacker (Prone and helpless criticals) still use
        // the caster distance; only cover originates at the remote force.
        int cover=effectOrigin.HasValue?(effectSource.HasValue?map.CreatureCoverAt(effectSource.Value,target,new CreatureCover.Body(effectOrigin.Value.x,effectOrigin.Value.y,effectOriginSpaces),new CreatureCover.Body(p.x,p.y,m)):map.CreatureCoverFromPoint(effectOrigin.Value,target)):map.CreatureCoverBonus(source,target),armor=(primary?(player?combat.PlayerArmorClass:combat.MonsterArmorClass):unit.Armor)+cover;
        BreakOrdinaryInvisibility(playerSource,"进行攻击检定");mirrorIntercepted=false;
        if(player&&value!=1&&(value==20||(long)value+bonus>=armor)){yield return ResolvePlayerHitDefense(value,(long)value+bonus,sees,PerceivesWithSpecialSense(playerSource,true),cover);armor=combat.PlayerArmorClass+cover;}
        result.Intercepted=mirrorIntercepted;result.Hit=!result.Intercepted&&value!=1&&(value==20||(long)value+bonus>=armor);result.Critical=result.Hit&&(value==20||helpless&&distance<=5);
        dice=title+(melee?" · 近战法术攻击":" · 远程法术攻击");yield return PresentRoll(20,second==0?new[]{first}:new[]{first,second},"采用 "+value+" + "+bonus+" / AC "+armor+(result.Intercepted?"，镜像拦截":result.Hit?result.Critical?"，暴击":"，命中":"，未命中"));
        if(bless>0)yield return PresentRoll(4,new[]{bless},"祝福术 +"+bless);if(bane>0)yield return PresentRoll(4,new[]{bane},"灾祸术 -"+bane);if(ward>0)yield return PresentRoll(4,new[]{ward},"剑刃防护 -"+ward);
        if(playerSource&&!result.Hit&&!result.Intercepted)fighter.RecordMiss(classes[0]);
    }
}
