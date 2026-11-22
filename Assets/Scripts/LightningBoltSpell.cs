using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class TrainingGame
{
    readonly BreathChoice lightningChoice=new BreathChoice();
    Vector2 lightningScroll;int lightningDirection=-1;bool lightningAimInitialized;
    bool PlayerLightningTemplate(out BreathTemplate template)
    {
        int spaces=map.PlayerSpaces;
        if(lightningChoice.Offset<0)lightningChoice.Offset=spaces;
        lightningChoice.Offset=Mathf.Clamp(lightningChoice.Offset,0,spaces*2);
        if(!lightningAimInitialized){lightningAimInitialized=true;lightningChoice.AimX=map.Player.x+spaces+20;lightningChoice.AimY=map.Player.y+spaces*.5;}
        return BreathTemplate.TryCreate(map.Player.x,map.Player.y,spaces,lightningChoice.Edge,lightningChoice.Offset,lightningChoice.AimX,lightningChoice.AimY,true,out template,lightningChoice.IncludeOrigin,LightningBoltRules.LengthFeet);
    }
    string LightningBoltError(){BreathTemplate template;return PlayerLightningTemplate(out template)?"":"请选择有效的线形源点和朝向；边缘源点必须向外。";}
    bool TryPlanMonsterLightningBolt(out BreathTemplate template)
    {
        var enemies=new List<BreathBody>();
        foreach(int id in BreathActors())if((id==0||id>0)&&!Feigning(id)&&!NeutralUndead(id)){
            Vector2Int cell;int spaces;string name;
            if((id!=0||!map.IsContained(true))&&HealingHandsTarget(id,out cell,out spaces,out name))enemies.Add(new BreathBody(id,cell.x,cell.y,spaces));
        }
        return BreathWeaponRules.ChooseLine(map.Size,map.Monster.x,map.Monster.y,map.MonsterSpaces,enemies,out template,LightningBoltRules.LengthFeet);
    }
    IEnumerator ResolveLightningBolt(bool player,int casterClass,int slot,BreathTemplate template)
    {
        int count=LightningBoltRules.DiceCount(slot);
        if(count==0||!template.Valid||!template.Line||template.LengthFeet!=LightningBoltRules.LengthFeet)yield break;
        int source=player?0:-1;var targets=BreathTargets(source,template);var failed=new HashSet<int>();
        long dc=player?8L+classes.ProficiencyBonus+AcidSplash.Modifier(attributes[SpellAbilityIndex(casterClass)]):MonsterSpellDc;
        bool oldPreview=burningPreview;var oldTemplate=burningTemplate;int oldSource=burningSource;
        burningPreview=true;burningTemplate=template;burningSource=source;outcomeDeferDepth++;
        try{
            foreach(int target in targets)yield return RollMappedDexteritySave(target,source,dc,failed,"闪电束");
            int damage=0;var rolls=new int[count];for(int i=0;i<count;i++){rolls[i]=Random.Range(1,7);damage+=rolls[i];}
            dice="闪电束 · 共享闪电伤害";
            yield return PresentRoll(6,rolls,count+"d6 = "+damage+"；豁免成功 "+damage/2+" 闪电");
            var reduced=new RayDamage();yield return ReduceRayDamage(player,damage,"闪电","闪电束共享伤害",reduced);damage=reduced.Amount;
            int playerDamage=0;
            foreach(int target in targets){int applied=ApplyMappedAreaDamage(target,source,failed.Contains(target)?damage:damage/2,7,"闪电束",failed.Contains(target));if(target==0)playerDamage=applied;}
            if(playerDamage>0)yield return MaintainConcentration(playerDamage);
            ReconcileFallenRescue();RefreshBestialSpirit();NoticeCastingInterruption();dice="闪电束 · "+targets.Count+"名目标结算完成";
            history.Insert(0,"闪电束：100尺长、5尺宽线形，"+count+"d6共享闪电伤害；范围内生物均受影响，成功减半后再结算抗性／免疫。水域不额外导电，不产生火焰或环境点燃。");
        }finally{outcomeDeferDepth--;burningPreview=oldPreview;burningTemplate=oldTemplate;burningSource=oldSource;CheckBattleOutcome();}
    }
}
