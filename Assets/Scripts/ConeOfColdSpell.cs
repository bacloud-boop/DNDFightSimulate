using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    string ConeOfColdError(){BreathTemplate template;return PlayerBurningTemplate(out template,ConeOfColdRules.LengthFeet)?"":"请选择有效的60尺锥形源点与朝外方向。";}
    bool TryPlanMonsterConeOfCold(out BreathTemplate template)
    {
        var enemies=new List<BreathBody>();
        foreach(int actor in BreathActors())if((actor==0||actor>0)&&!Feigning(actor)&&!NeutralUndead(actor)){
            Vector2Int p;int n;string name;if((actor!=0||!map.IsContained(true))&&HealingHandsTarget(actor,out p,out n,out name))enemies.Add(new BreathBody(actor,p.x,p.y,n));
        }
        return BreathWeaponRules.Choose(map.Size,map.Monster.x,map.Monster.y,map.MonsterSpaces,enemies,out template,false,ConeOfColdRules.LengthFeet);
    }
    bool ConeTargetDead(int actor)=>actor==0?combat.PlayerDead:actor==-1?combat.Hp==0:actor==BattleMap.RescueId?rescueCarrier.Dead:FindSummon(actor)==null||!FindSummon(actor).Alive;
    void FreezeConeVictim(int actor)
    {
        ObserveDeaths();var corpse=CurrentCorpse(actor);
        if(corpse!=null){if(corpse.Freeze())history.Insert(0,"寒冰锥："+TransitActorName(actor)+"遗体冻结；可免费手动解冻，重量、搬运和复活照常。");return;}
        var follower=Undead(FindSummon(actor));if(follower==null)return; // Spirits disappearing at 0 HP leave no body.
        RefreshUndeadFollowers();var remains=map.Remains.Find(follower.Corpse);
        if(remains!=null){remains.Frozen=true;history.Insert(0,"寒冰锥："+remains.Name+"冻结，可免费手动解冻。");}
    }
    IEnumerator ResolveConeOfCold(bool player,int caster,int slot,BreathTemplate template)
    {
        int count=ConeOfColdRules.DiceCount(slot);if(count==0||!template.Valid||template.Line||template.LengthFeet!=ConeOfColdRules.LengthFeet)yield break;
        int source=player?0:-1;var targets=BreathTargets(source,template);var failed=new HashSet<int>();
        long dc=player?8L+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(caster)]):MonsterSpellDc;
        bool oldPreview=burningPreview;var oldTemplate=burningTemplate;int oldSource=burningSource;
        burningPreview=true;burningTemplate=template;burningSource=source;outcomeDeferDepth++;
        try{
            foreach(int actor in targets)yield return RollMappedAbilitySave(actor,source,dc,failed,"寒冰锥",2);
            int damage=0;var rolls=new int[count];for(int i=0;i<count;i++){rolls[i]=Random.Range(1,9);damage+=rolls[i];}
            dice="寒冰锥 · 共享寒冷伤害";yield return PresentRoll(8,rolls,count+"d8 = "+damage+"，豁免成功 "+damage/2);
            var reduced=new RayDamage();yield return ReduceRayDamage(player,damage,"寒冷","寒冰锥共享伤害",reduced);
            int self=0;foreach(int actor in targets){
                if(ConeTargetDead(actor))continue;
                var packet=new long[13];packet[4]=ConeOfColdRules.AfterSave(reduced.Amount,!failed.Contains(actor));
                int amount=ApplyMappedDamagePacket(actor,source,packet);if(actor==0)self=amount;
                history.Insert(0,"寒冰锥："+TransitActorName(actor)+(failed.Contains(actor)?"体质豁免失败":"体质豁免成功，伤害减半")+"，结算 "+amount+" 寒冷。");
                if(ConeTargetDead(actor))FreezeConeVictim(actor);
            }
            yield return MaintainConcentration(self);ReconcileFallenRescue();RefreshUndeadFollowers();RefreshBestialSpirit();NoticeCastingInterruption();
            dice="寒冰锥 · "+targets.Count+"名目标结算完成";
        }finally{outcomeDeferDepth--;burningPreview=oldPreview;burningTemplate=oldTemplate;burningSource=oldSource;CheckBattleOutcome();}
    }
}
