using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly DragonBreathEffect dragonBreath=new DragonBreathEffect();
    static readonly int[] DragonBreathTypes={3,4,5,7,9};
    int dragonBreathTarget,dragonBreathType;bool dragonBreathWilling;
    string DragonBreathError()
    {
        Vector2Int p;int n;string name;if(dragonBreathTarget>0&&(FindSummon(dragonBreathTarget)==null||!FindSummon(dragonBreathTarget).Alive)||!HealingHandsTarget(dragonBreathTarget,out p,out n,out name)||!HealingHandsCanTouch(0,dragonBreathTarget))return "请选择接触范围内仍存活的目标。";
        if(dragonBreathTarget!=0&&!dragonBreathWilling)return "请确认目标自愿接受龙息术。";
        if(bestialSpirit.Active&&dragonBreathTarget==bestialSpirit.Unit.Id&&concentrationSpell=="召唤野兽")return "无法同时专注召唤野兽与龙息术；开始新专注会令该灵魄消失。";
        return worldRounds<0||worldRounds>long.MaxValue-10?"世界时间超出范围。":"";
    }
    void DrawDragonBreathChoices()
    {
        foreach(int actor in BreathActors())if(GUILayout.Button((dragonBreathTarget==actor?"✓ ":"")+TransitActorName(actor)+(HealingHandsCanTouch(0,actor)?"":"（无法接触）"),button))dragonBreathTarget=actor;
        if(dragonBreathTarget!=0)dragonBreathWilling=GUILayout.Toggle(dragonBreathWilling,"目标自愿接受",label);
        dragonBreathType=GUILayout.SelectionGrid(dragonBreathType,new[]{"强酸","寒冷","火焰","闪电","毒素"},3,button);
        GUILayout.Label("附赠动作施放，专注至多1分钟；目标可反复消耗魔法动作喷吐15尺锥形，"+DragonBreathEffect.DiceCount(directSlotLevel)+"d6所选伤害，敏捷成功减半。喷吐不是再次施法，无需声音、姿势或空手，不触发额外攻击。",small);
    }
    void CastDragonBreath(int caster,int target,int slot,int type)
    {
        Vector2Int p;int n;string name;if(!HealingHandsTarget(target,out p,out n,out name)||!HealingHandsCanTouch(0,target)){history.Insert(0,"龙息术目标已不在接触范围，法术未生效。");return;}
        long dc=8L+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]);
        if(!dragonBreath.Begin(target,slot,type,dc,worldRounds))return;
        concentrationSpell="龙息术";concentrationExpiresRound=int.MaxValue;concentrationExpiresWorld=dragonBreath.Expires;
        history.Insert(0,"龙息术："+name+"获得15尺锥形喷吐，"+DragonBreathEffect.DiceCount(slot)+"d6 "+DamageTypes.Names[type]+"，敏捷DC "+dc+"；玩家维持专注至多1分钟。");
    }
    bool DragonBreathReady(int actor)
    {
        if(page!="battle"||!map.Locked||!dragonBreath.Active(worldRounds)||dragonBreath.Target!=actor||concentrationSpell!="龙息术")return false;
        if(actor==0)return PlayerTurnAllowed&&!summonTurns.Active&&combat.PlayerHp>0&&!combat.PlayerDead&&!combat.PlayerParalyzed&&!combat.PlayerFeignDeath&&(combat.Aftermath?combat.AftermathMagicActionAvailable:combat.MagicActionAvailable);
        if(actor==-1)return combat.MonsterCanAct&&combat.MonsterActionAvailable;
        if(actor==BattleMap.RescueId)return RescueTurnAllowed&&rescueCarrier.Conscious&&rescueCarrier.TurnActive&&rescueCarrier.ActionsRemaining>0;
        var unit=FindSummon(actor);return unit!=null&&unit.Alive&&summonTurns.Current==unit&&unit.TurnActive&&unit.ActionAvailable&&(!IsSteed(unit)||!ControlledSteed);
    }
    bool TryPlanDragonBreath(int source,out BreathTemplate template)
    {
        Vector2Int p;int n;string name;template=default(BreathTemplate);if(!HealingHandsTarget(source,out p,out n,out name))return false;
        var enemies=new List<BreathBody>();foreach(int actor in BreathActors())if((source==-1?(actor==0||actor>0):actor==-1)&&!Feigning(actor)&&!NeutralUndead(actor)){
            Vector2Int q;int m;if((actor!=0&&actor!=-1||!map.IsContained(actor==0))&&HealingHandsTarget(actor,out q,out m,out name))enemies.Add(new BreathBody(actor,q.x,q.y,m));
        }
        return BreathWeaponRules.Choose(map.Size,p.x,p.y,n,enemies,out template,false);
    }
    IEnumerator UseDragonBreath(int actor,BreathTemplate template,bool automatic=false)
    {
        if(!DragonBreathReady(actor)||!template.Valid||template.Line||template.LengthFeet!=15||!automatic&&(rolling&&!(actor>0&&summonTurns.Current==FindSummon(actor))||summonActing||spellTargeting||reactionPending||draggingToken>=0))yield break;
        bool paid=actor==0?(combat.Aftermath?combat.TryUseAftermathMagicAction():combat.TryUseMagicAction()):actor==-1?combat.TryUseMonsterAction():actor==BattleMap.RescueId?rescueCarrier.UseAction():FindSummon(actor).TryUseAction();
        if(!paid)yield break;
        int count=DragonBreathEffect.DiceCount(dragonBreath.Slot),type=dragonBreath.Type;long dc=dragonBreath.Dc;
        var targets=BreathTargets(actor,template);var failed=new HashSet<int>();bool busy=rolling,acting=summonActing;string saved=panel;
        bool oldPreview=burningPreview;var oldTemplate=burningTemplate;int oldSource=burningSource;bool oldFire=webAreaFire;
        rolling=true;if(actor>0)summonActing=true;panel="";burningPreview=true;burningTemplate=template;burningSource=actor;outcomeDeferDepth++;
        try{
            history.Insert(0,TransitActorName(actor)+"消耗魔法动作使用龙息术喷吐；本次不是施法，不消耗法术位，不需要成分。");
            foreach(int target in targets)yield return RollMappedDexteritySave(target,actor,dc,failed,"龙息术喷吐");
            int sum=0;var rolls=new int[count];for(int i=0;i<count;i++){rolls[i]=Random.Range(1,7);sum+=rolls[i];}
            dice="龙息术喷吐 · 共享伤害";yield return PresentRoll(6,rolls,count+"d6 "+DamageTypes.Names[type]+" = "+sum+"，豁免成功 "+sum/2);
            if(actor==0||actor==-1){var reduced=new RayDamage();yield return ReduceRayDamage(actor==0,sum,DamageTypes.Names[type],"龙息术喷吐",reduced);sum=reduced.Amount;}
            if(type==5){if((actor!=0&&actor!=-1)||!map.IsContained(actor==0))IgniteWebArea(template.Cell);webAreaFire=true;}
            int self=0;foreach(int target in targets){int amount=ApplyMappedAreaDamage(target,actor,failed.Contains(target)?sum:sum/2,type,"龙息术喷吐",failed.Contains(target));if(target==0)self=amount;}
            webAreaFire=oldFire;yield return MaintainConcentration(self);ReconcileFallenRescue();RefreshUndeadFollowers();RefreshBestialSpirit();dice="龙息术喷吐 · "+targets.Count+"名目标已结算";
        }finally{webAreaFire=oldFire;outcomeDeferDepth--;burningPreview=oldPreview;burningTemplate=oldTemplate;burningSource=oldSource;panel=automatic?"":saved;rolling=busy;summonActing=acting;CheckBattleOutcome();}
    }
}
